import { CdkDrag, CdkDragEnd } from '@angular/cdk/drag-drop';
import { Component, inject, OnInit, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { GraphItemComponent } from '../../components/graph-item/graph-item.component';
import { confirmDeleteGraph } from '../../components/confirm-dialog/confirm-dialog.component';
import { GraphItem } from '../../models/graph-item.model';
import { GraphItemService } from '../../services/graph-item.service';
import { GraphWizardDialogComponent, GraphWizardCloseResult } from '../../wizard/graph-wizard-dialog.component';

@Component({
  selector: 'app-dashboard',
  imports: [
    CdkDrag,
    GraphItemComponent,
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
  private readonly graphItemService = inject(GraphItemService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  readonly graphItems = signal<GraphItem[]>([]);
  readonly isLoading = signal(true);
  readonly hasError = signal(false);
  readonly isRearranging = signal(false);
  readonly isSavingOrder = signal(false);

  private originalItems: GraphItem[] = [];

  ngOnInit(): void {
    this.loadGraphItems();
  }

  onCreateGraph(): void {
    if (this.isRearranging()) {
      return;
    }

    this.openWizard();
  }

  onEditGraph(item: GraphItem): void {
    if (this.isRearranging()) {
      return;
    }

    this.openWizard(item);
  }

  onDeleteGraph(item: GraphItem): void {
    if (this.isRearranging()) {
      return;
    }

    confirmDeleteGraph(this.dialog).subscribe((confirmed) => {
      if (!confirmed) {
        return;
      }

      this.graphItemService.deleteGraphItem(item.id).subscribe({
        next: () => {
          this.snackBar.open('Graph deleted successfully.', 'Dismiss', { duration: 4000 });
          this.loadGraphItems();
        },
        error: () => {
          this.snackBar.open('Could not delete the graph. Please try again.', 'Dismiss', {
            duration: 4000
          });
        }
      });
    });
  }

  onStartRearrange(): void {
    this.originalItems = [...this.graphItems()];
    this.isRearranging.set(true);
  }

  onAbortRearrange(): void {
    this.graphItems.set([...this.originalItems]);
    this.originalItems = [];
    this.isRearranging.set(false);
    this.isSavingOrder.set(false);
  }

  onSaveOrder(): void {
    const items = this.graphItems();
    const payload = items.map((item, index) => ({
      graphId: item.id,
      ordering: index + 1
    }));

    this.isSavingOrder.set(true);
    this.graphItemService.updateGraphOrdering(payload).subscribe({
      next: () => {
        this.graphItems.set(
          items.map((item, index) => ({
            ...item,
            ordering: index + 1
          }))
        );
        this.originalItems = [];
        this.isRearranging.set(false);
        this.isSavingOrder.set(false);
        this.snackBar.open('Graph order saved.', 'Dismiss', { duration: 4000 });
      },
      error: () => {
        this.isSavingOrder.set(false);
        this.snackBar.open('Could not save the graph order. Please try again.', 'Dismiss', {
          duration: 4000
        });
      }
    });
  }

  onDragEnded(event: CdkDragEnd<GraphItem>): void {
    const dragged = event.source.data;
    const { x, y } = event.dropPoint;
    const grid = event.source.element.nativeElement.closest('.dashboard-grid');
    const slots = grid ? Array.from(grid.querySelectorAll<HTMLElement>(':scope > .grid-item')) : [];
    const targetIndex = slots.findIndex((slot) => {
      const rect = slot.getBoundingClientRect();
      return x >= rect.left && x <= rect.right && y >= rect.top && y <= rect.bottom;
    });

    event.source.reset();

    if (!dragged) {
      return;
    }

    this.graphItems.update((items) => {
      const from = items.findIndex((item) => item.id === dragged.id);
      if (from < 0 || targetIndex < 0 || from === targetIndex) {
        return items;
      }

      const next = [...items];
      [next[from], next[targetIndex]] = [next[targetIndex], next[from]];
      return next;
    });
  }

  retry(): void {
    this.loadGraphItems();
  }

  private openWizard(graphItem?: GraphItem): void {
    this.dialog
      .open<GraphWizardDialogComponent, { graphItem?: GraphItem }, GraphWizardCloseResult>(
        GraphWizardDialogComponent,
        {
          width: 'min(52rem, calc(100vw - 2rem))',
          maxWidth: '52rem',
          autoFocus: 'first-tabbable',
          restoreFocus: true,
          panelClass: 'graph-wizard-dialog',
          data: graphItem ? { graphItem } : {}
        }
      )
      .afterClosed()
      .subscribe((result) => this.applyWizardResult(result));
  }

  private applyWizardResult(result: GraphWizardCloseResult | undefined): void {
    if (!result) {
      return;
    }

    if (result.action === 'deleted') {
      this.snackBar.open('Graph deleted successfully.', 'Dismiss', { duration: 4000 });
      this.loadGraphItems();
      return;
    }

    if (result.created) {
      this.graphItems.update((items) => [...items, result.item]);
      this.snackBar.open('Graph created successfully.', 'Dismiss', { duration: 4000 });
      return;
    }

    this.graphItems.update((items) =>
      items.map((item) => (item.id === result.item.id ? result.item : item))
    );
    this.snackBar.open('Graph updated successfully.', 'Dismiss', { duration: 4000 });
  }

  private loadGraphItems(): void {
    this.isLoading.set(true);
    this.hasError.set(false);
    this.isRearranging.set(false);
    this.isSavingOrder.set(false);
    this.originalItems = [];

    this.graphItemService.getGraphItems().subscribe({
      next: (items) => {
        this.graphItems.set(items);
        this.isLoading.set(false);
      },
      error: () => {
        this.hasError.set(true);
        this.isLoading.set(false);
      }
    });
  }
}
