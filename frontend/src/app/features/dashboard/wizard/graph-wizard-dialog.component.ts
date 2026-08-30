import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, OnInit, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialog, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { map, startWith } from 'rxjs';
import { serializeGraphItemContent } from '../models/build-graph-item-content';
import { GraphItem } from '../models/graph-item.model';
import { GraphItemService } from '../services/graph-item.service';
import { confirmDeleteGraph } from '../components/confirm-dialog/confirm-dialog.component';
import { ConfigureGraphStepComponent } from './configure-graph-step/configure-graph-step.component';
import { GraphWizardCatalog } from './graph-wizard-catalog';
import { GraphWizardState } from './graph-wizard-state';
import { ReviewGraphStepComponent } from './review-graph-step/review-graph-step.component';
import { SelectDataStepComponent } from './select-data-step/select-data-step.component';
import { SelectGraphTypeStepComponent } from './select-graph-type-step/select-graph-type-step.component';

export interface GraphWizardDialogData {
  graphItem?: GraphItem;
}

export type GraphWizardCloseResult =
  | { action: 'saved'; item: GraphItem; created: boolean }
  | { action: 'deleted'; id: number };

@Component({
  selector: 'app-graph-wizard-dialog',
  imports: [
    MatButtonModule,
    MatDialogModule,
    MatIconModule,
    ConfigureGraphStepComponent,
    ReviewGraphStepComponent,
    SelectDataStepComponent,
    SelectGraphTypeStepComponent
  ],
  providers: [GraphWizardState, GraphWizardCatalog],
  templateUrl: './graph-wizard-dialog.component.html',
  styleUrl: './graph-wizard-dialog.component.scss'
})
export class GraphWizardDialogComponent implements OnInit {
  private readonly dialogRef = inject(MatDialogRef<GraphWizardDialogComponent, GraphWizardCloseResult>);
  private readonly dialog = inject(MatDialog);
  private readonly graphItemService = inject(GraphItemService);
  private readonly data = inject<GraphWizardDialogData | null>(MAT_DIALOG_DATA, { optional: true });
  readonly state = inject(GraphWizardState);
  readonly catalog = inject(GraphWizardCatalog);

  readonly step = signal<1 | 2 | 3 | 4>(1);
  readonly isSaving = signal(false);
  readonly saveError = signal<string | null>(null);

  readonly formInvalid = toSignal(
    this.state.form.statusChanges.pipe(
      startWith(this.state.form.status),
      map(() => this.state.form.invalid)
    ),
    { initialValue: this.state.form.invalid }
  );

  ngOnInit(): void {
    const item = this.data?.graphItem;
    if (item) {
      this.state.hydrate(item);
    }

    this.catalog.load();
  }

  close(): void {
    this.dialogRef.close();
  }

  goBack(): void {
    this.saveError.set(null);
    const current = this.step();
    if (current === 4) {
      this.step.set(3);
      return;
    }

    if (current === 3) {
      this.step.set(2);
      return;
    }

    this.step.set(1);
  }

  continue(): void {
    this.saveError.set(null);

    if (this.step() === 1 && this.state.selectedGraphDataType() !== null) {
      this.step.set(2);
      return;
    }

    if (this.step() === 2 && this.state.selectedGraphType() !== null) {
      this.step.set(3);
      return;
    }

    if (this.step() === 3 && this.state.form.valid) {
      this.step.set(4);
    }
  }

  nextDisabled(): boolean {
    if (this.step() === 2) {
      return this.state.selectedGraphType() === null;
    }

    if (this.step() === 3) {
      return this.formInvalid() ?? true;
    }

    return true;
  }

  saveGraph(): void {
    const dataType = this.state.selectedGraphDataType();
    const graphType = this.state.selectedGraphType();

    if (!dataType || !graphType || this.isSaving()) {
      return;
    }

    const graphTypeId = this.catalog.graphTypeId(graphType);
    const graphDataTypeId = this.catalog.graphDataTypeId(dataType);

    if (graphTypeId == null || graphDataTypeId == null) {
      this.saveError.set(
        'Graph metadata is not available. Check that the API is running, then try again.'
      );
      return;
    }

    const value = this.state.form.getRawValue();
    const editingId = this.state.graphItemId();
    this.isSaving.set(true);
    this.saveError.set(null);

    this.graphItemService
      .upsertGraphItem({
        id: editingId ?? undefined,
        name: value.name.trim(),
        description: value.description.trim() || null,
        graphTypeId,
        graphDataTypeId,
        content: serializeGraphItemContent(dataType, value)
      })
      .subscribe({
        next: (item) =>
          this.dialogRef.close({ action: 'saved', item, created: editingId == null }),
        error: (error: unknown) => {
          this.isSaving.set(false);
          this.saveError.set(this.readErrorMessage(error, 'save'));
        }
      });
  }

  requestDelete(): void {
    this.saveError.set(null);
    const id = this.state.graphItemId();
    if (id == null || this.isSaving()) {
      return;
    }

    confirmDeleteGraph(this.dialog).subscribe((confirmed) => {
      if (!confirmed || this.isSaving()) {
        return;
      }

      this.isSaving.set(true);
      this.graphItemService.deleteGraphItem(id).subscribe({
        next: () => this.dialogRef.close({ action: 'deleted', id }),
        error: (error: unknown) => {
          this.isSaving.set(false);
          this.saveError.set(this.readErrorMessage(error, 'delete'));
        }
      });
    });
  }

  private readErrorMessage(error: unknown, mode: 'save' | 'delete'): string {
    if (error instanceof HttpErrorResponse) {
      const payload = error.error as { message?: string } | null;
      if (payload && typeof payload.message === 'string' && payload.message.length > 0) {
        return payload.message;
      }

      if (error.status === 0) {
        return 'Unable to reach the API. Check that it is running, then try again.';
      }
    }

    return mode === 'delete'
      ? 'Could not delete the graph. Please try again.'
      : this.state.isEditing()
        ? 'Could not update the graph. Please try again.'
        : 'Could not create the graph. Please try again.';
  }
}
