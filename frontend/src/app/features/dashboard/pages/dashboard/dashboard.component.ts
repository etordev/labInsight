import { CdkDrag, CdkDragEnd, CdkDragMove, CdkDragStart, moveItemInArray } from '@angular/cdk/drag-drop';
import { ChangeDetectorRef, Component, inject, OnInit, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { DashboardWidgetComponent } from '../../components/dashboard-widget/dashboard-widget.component';
import { confirmDeleteDashboardWidget } from '../../components/confirm-dialog/confirm-dialog.component';
import { openHowItWorksDialog } from '../../components/how-it-works-dialog/how-it-works-dialog.component';
import { DashboardWidget } from '../../models/dashboard-widget.model';
import { DashboardWidgetService } from '../../services/dashboard-widget.service';
import { DashboardWidgetWizardDialogComponent, DashboardWidgetWizardCloseResult } from '../../wizard/dashboard-widget-wizard-dialog.component';

@Component({
  selector: 'app-dashboard',
  imports: [
    CdkDrag,
    DashboardWidgetComponent,
    MatButtonModule,
    MatDialogModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  private readonly dashboardWidgetService = inject(DashboardWidgetService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly changeDetectorRef = inject(ChangeDetectorRef);

  readonly dashboardWidgets = signal<DashboardWidget[]>([]);
  readonly isLoading = signal(true);
  readonly hasError = signal(false);
  readonly isRearranging = signal(false);
  readonly isSavingOrder = signal(false);
  readonly draggingId = signal<number | null>(null);
  readonly displayOrder = signal<number[]>([]);

  private originalItems: DashboardWidget[] = [];
  private isShiftingSlots = false;
  private liveOrder: number[] = [];
  private isDragViewDetached = false;
  private displacedDashboardWidgetId: number | null = null;
  private readonly coverToMove = 0.3;
  private readonly coverToRelease = 0.15;

  ngOnInit(): void {
    this.loadDashboardWidgets();
  }

  onHowItWorks(): void {
    openHowItWorksDialog(this.dialog);
  }

  onCreateDashboardWidget(): void {
    if (this.isRearranging()) {
      return;
    }

    this.openWizard();
  }

  onEditDashboardWidget(item: DashboardWidget): void {
    if (this.isRearranging()) {
      return;
    }

    this.openWizard(item);
  }

  onDashboardWidgetUpdated(updated: DashboardWidget): void {
    this.dashboardWidgets.update((items) =>
      items.map((item) => (item.id === updated.id ? updated : item))
    );
  }

  onDeleteDashboardWidget(item: DashboardWidget): void {
    if (this.isRearranging()) {
      return;
    }

    confirmDeleteDashboardWidget(this.dialog).subscribe((confirmed) => {
      if (!confirmed) {
        return;
      }

      this.dashboardWidgetService.deleteDashboardWidget(item.id).subscribe({
        next: () => {
          this.snackBar.open('Widget deleted successfully.', 'Dismiss', { duration: 4000 });
          this.loadDashboardWidgets();
        },
        error: () => {
          this.snackBar.open('Could not delete the widget. Please try again.', 'Dismiss', {
            duration: 4000
          });
        }
      });
    });
  }

  onStartRearrange(): void {
    this.originalItems = [...this.dashboardWidgets()];
    this.displayOrder.set(this.dashboardWidgets().map((item) => item.id));
    this.isRearranging.set(true);
  }

  onAbortRearrange(): void {
    this.dashboardWidgets.set([...this.originalItems]);
    this.displayOrder.set(this.originalItems.map((item) => item.id));
    this.originalItems = [];
    this.isRearranging.set(false);
    this.isSavingOrder.set(false);
    this.draggingId.set(null);
    this.reattachDragView();
  }

  onSaveOrder(): void {
    const items = this.itemsInDisplayOrder();
    const payload = items.map((item, index) => ({
      dashboardWidgetId: item.id,
      ordering: index + 1
    }));

    this.isSavingOrder.set(true);
    this.dashboardWidgetService.updateDashboardWidgetOrdering(payload).subscribe({
      next: () => {
        this.dashboardWidgets.set(
          items.map((item, index) => ({
            ...item,
            ordering: index + 1
          }))
        );
        this.displayOrder.set(items.map((item) => item.id));
        this.originalItems = [];
        this.isRearranging.set(false);
        this.isSavingOrder.set(false);
        this.draggingId.set(null);
        this.snackBar.open('Widget order saved.', 'Dismiss', { duration: 4000 });
      },
      error: () => {
        this.isSavingOrder.set(false);
        this.snackBar.open('Could not save the widget order. Please try again.', 'Dismiss', {
          duration: 4000
        });
      }
    });
  }

  displayIndex(id: number): number {
    const index = this.displayOrder().indexOf(id);
    return index < 0 ? 0 : index;
  }

  onDragStarted(event: CdkDragStart<DashboardWidget>): void {
    this.isShiftingSlots = false;
    this.displacedDashboardWidgetId = null;
    this.liveOrder = [...this.displayOrder()];
    this.draggingId.set(event.source.data?.id ?? null);
    this.pinDraggedCard(event.source.getRootElement());
    this.changeDetectorRef.detectChanges();
    this.changeDetectorRef.detach();
    this.isDragViewDetached = true;
  }

  onDragMoved(event: CdkDragMove<DashboardWidget>): void {
    if (this.isShiftingSlots || !this.isRearranging()) {
      return;
    }

    const dragged = event.source.data;
    if (!dragged) {
      return;
    }

    const grid = event.source.getRootElement().closest('.dashboard-grid');
    if (!(grid instanceof HTMLElement)) {
      return;
    }

    const from = this.liveOrder.indexOf(dragged.id);
    const targetId = this.targetDashboardWidgetId(grid, event.source.getRootElement(), dragged.id);
    const to = targetId == null ? -1 : this.liveOrder.indexOf(targetId);
    if (from < 0 || to < 0 || from === to) {
      return;
    }

    const originRects = this.slotRectsByItemId(grid);
    this.isShiftingSlots = true;
    moveItemInArray(this.liveOrder, from, to);
    this.displacedDashboardWidgetId = targetId;
    this.applySlotOrder(grid);
    this.animateSlotShift(grid, dragged.id, originRects);
    this.isShiftingSlots = false;
  }

  onDragEnded(event: CdkDragEnd<DashboardWidget>): void {
    this.unpinDraggedCard(event.source.getRootElement());
    event.source.reset();
    this.isShiftingSlots = false;
    this.displacedDashboardWidgetId = null;
    this.draggingId.set(null);
    this.displayOrder.set([...this.liveOrder]);
    this.dashboardWidgets.set(this.itemsInDisplayOrder());
    this.reattachDragView();
  }

  retry(): void {
    this.loadDashboardWidgets();
  }

  private pinDraggedCard(element: HTMLElement): void {
    const currentTransform = element.style.transform;
    element.style.transform = 'none';
    const rect = element.getBoundingClientRect();
    element.style.position = 'fixed';
    element.style.left = `${rect.left}px`;
    element.style.top = `${rect.top}px`;
    element.style.width = `${rect.width}px`;
    element.style.height = `${rect.height}px`;
    element.style.margin = '0';
    element.style.zIndex = '20';
    element.style.transform = currentTransform;
  }

  private unpinDraggedCard(element: HTMLElement): void {
    element.style.position = '';
    element.style.left = '';
    element.style.top = '';
    element.style.width = '';
    element.style.height = '';
    element.style.margin = '';
    element.style.zIndex = '';
    element.style.transform = '';
  }

  private targetDashboardWidgetId(
    grid: HTMLElement,
    draggedCard: HTMLElement,
    draggedId: number
  ): number | null {
    const draggedRect = draggedCard.getBoundingClientRect();
    let bestId: number | null = null;
    let bestCoverage = this.coverToMove;

    for (const slot of this.gridSlots(grid)) {
      const id = Number(slot.dataset['dashboardWidgetId']);
      if (Number.isNaN(id) || id === draggedId) {
        continue;
      }

      const coverage = this.overlapRatio(draggedRect, slot.getBoundingClientRect());
      if (id === this.displacedDashboardWidgetId) {
        if (coverage < this.coverToRelease) {
          this.displacedDashboardWidgetId = null;
        }
        continue;
      }

      if (coverage >= bestCoverage) {
        bestCoverage = coverage;
        bestId = id;
      }
    }

    return bestId;
  }

  private overlapRatio(dragged: DOMRectReadOnly, target: DOMRectReadOnly): number {
    const targetArea = target.width * target.height;
    if (targetArea <= 0) {
      return 0;
    }

    const width = Math.min(dragged.right, target.right) - Math.max(dragged.left, target.left);
    const height = Math.min(dragged.bottom, target.bottom) - Math.max(dragged.top, target.top);
    if (width <= 0 || height <= 0) {
      return 0;
    }

    return (width * height) / targetArea;
  }

  private slotRectsByItemId(grid: HTMLElement): Map<number, DOMRect> {
    const rects = new Map<number, DOMRect>();
    this.gridSlots(grid).forEach((slot) => {
      const id = Number(slot.dataset['dashboardWidgetId']);
      if (!Number.isNaN(id)) {
        rects.set(id, slot.getBoundingClientRect());
      }
    });
    return rects;
  }

  private animateSlotShift(
    grid: HTMLElement,
    draggedId: number,
    originRects: Map<number, DOMRect>
  ): void {
    this.gridSlots(grid).forEach((slot) => {
      const id = Number(slot.dataset['dashboardWidgetId']);
      if (Number.isNaN(id) || id === draggedId) {
        return;
      }

      const previous = originRects.get(id);
      if (!previous) {
        return;
      }

      const next = slot.getBoundingClientRect();
      const deltaX = previous.left - next.left;
      const deltaY = previous.top - next.top;
      if (deltaX === 0 && deltaY === 0) {
        return;
      }

      slot.style.transition = 'none';
      slot.style.transform = `translate3d(${deltaX}px, ${deltaY}px, 0)`;
      void slot.offsetWidth;
      slot.style.transition = 'transform 200ms ease';
      slot.style.transform = 'translate3d(0, 0, 0)';
      slot.addEventListener(
        'transitionend',
        () => {
          slot.style.transition = '';
          slot.style.transform = '';
        },
        { once: true }
      );
    });
  }

  private applySlotOrder(grid: HTMLElement): void {
    this.gridSlots(grid).forEach((slot) => {
      const id = Number(slot.dataset['dashboardWidgetId']);
      if (Number.isNaN(id)) {
        return;
      }

      slot.style.order = String(this.liveOrder.indexOf(id));
    });
  }

  private reattachDragView(): void {
    if (!this.isDragViewDetached) {
      return;
    }

    this.changeDetectorRef.reattach();
    this.isDragViewDetached = false;
    this.changeDetectorRef.detectChanges();
  }

  private itemsInDisplayOrder(): DashboardWidget[] {
    const itemsById = new Map(this.dashboardWidgets().map((item) => [item.id, item]));
    return this.displayOrder()
      .map((id) => itemsById.get(id))
      .filter((item): item is DashboardWidget => item != null);
  }

  private gridSlots(grid: HTMLElement): HTMLElement[] {
    return Array.from(grid.querySelectorAll<HTMLElement>(':scope > .grid-item'));
  }

  private openWizard(dashboardWidget?: DashboardWidget): void {
    this.dialog
      .open<DashboardWidgetWizardDialogComponent, { dashboardWidget?: DashboardWidget }, DashboardWidgetWizardCloseResult>(
        DashboardWidgetWizardDialogComponent,
        {
          width: 'min(52rem, calc(100vw - 2rem))',
          maxWidth: '52rem',
          autoFocus: 'first-tabbable',
          restoreFocus: true,
          panelClass: 'dashboard-widget-wizard-dialog',
          data: dashboardWidget ? { dashboardWidget } : {}
        }
      )
      .afterClosed()
      .subscribe((result) => this.applyWizardResult(result));
  }

  private applyWizardResult(result: DashboardWidgetWizardCloseResult | undefined): void {
    if (!result) {
      return;
    }

    if (result.action === 'deleted') {
      this.snackBar.open('Widget deleted successfully.', 'Dismiss', { duration: 4000 });
      this.loadDashboardWidgets();
      return;
    }

    if (result.created) {
      this.dashboardWidgets.update((items) => [...items, result.item]);
      this.displayOrder.update((order) => [...order, result.item.id]);
      this.snackBar.open('Widget created successfully.', 'Dismiss', { duration: 4000 });
      return;
    }

    this.dashboardWidgets.update((items) =>
      items.map((item) => (item.id === result.item.id ? result.item : item))
    );
    this.snackBar.open('Widget updated successfully.', 'Dismiss', { duration: 4000 });
  }

  private loadDashboardWidgets(): void {
    this.isLoading.set(true);
    this.hasError.set(false);
    this.isRearranging.set(false);
    this.isSavingOrder.set(false);
    this.draggingId.set(null);
    this.reattachDragView();
    this.originalItems = [];

    this.dashboardWidgetService.getDashboardWidgets().subscribe({
      next: (items) => {
        this.dashboardWidgets.set(items);
        this.displayOrder.set(items.map((item) => item.id));
        this.isLoading.set(false);
      },
      error: () => {
        this.hasError.set(true);
        this.isLoading.set(false);
      }
    });
  }
}
