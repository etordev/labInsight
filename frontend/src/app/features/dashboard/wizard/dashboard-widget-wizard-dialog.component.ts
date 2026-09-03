import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, OnInit, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialog, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { map, startWith } from 'rxjs';
import { serializeDashboardWidgetContent } from '../models/build-dashboard-widget-content';
import { DashboardWidget } from '../models/dashboard-widget.model';
import { DashboardWidgetService } from '../services/dashboard-widget.service';
import { confirmDeleteDashboardWidget } from '../components/confirm-dialog/confirm-dialog.component';
import { ConfigureDashboardWidgetStepComponent } from './configure-dashboard-widget-step/configure-dashboard-widget-step.component';
import { DashboardWidgetWizardCatalog } from './dashboard-widget-wizard-catalog';
import { DashboardWidgetWizardState } from './dashboard-widget-wizard-state';
import { ReviewDashboardWidgetStepComponent } from './review-dashboard-widget-step/review-dashboard-widget-step.component';
import { SelectDataStepComponent } from './select-data-step/select-data-step.component';
import { SelectVisualizationTypeStepComponent } from './select-visualization-type-step/select-visualization-type-step.component';

export interface DashboardWidgetWizardDialogData {
  dashboardWidget?: DashboardWidget;
}

export type DashboardWidgetWizardCloseResult =
  | { action: 'saved'; item: DashboardWidget; created: boolean }
  | { action: 'deleted'; id: number };

@Component({
  selector: 'app-dashboard-widget-wizard-dialog',
  imports: [
    MatButtonModule,
    MatDialogModule,
    MatIconModule,
    ConfigureDashboardWidgetStepComponent,
    ReviewDashboardWidgetStepComponent,
    SelectDataStepComponent,
    SelectVisualizationTypeStepComponent
  ],
  providers: [DashboardWidgetWizardState, DashboardWidgetWizardCatalog],
  templateUrl: './dashboard-widget-wizard-dialog.component.html',
  styleUrl: './dashboard-widget-wizard-dialog.component.scss'
})
export class DashboardWidgetWizardDialogComponent implements OnInit {
  private readonly dialogRef = inject(MatDialogRef<DashboardWidgetWizardDialogComponent, DashboardWidgetWizardCloseResult>);
  private readonly dialog = inject(MatDialog);
  private readonly dashboardWidgetService = inject(DashboardWidgetService);
  private readonly data = inject<DashboardWidgetWizardDialogData | null>(MAT_DIALOG_DATA, { optional: true });
  readonly state = inject(DashboardWidgetWizardState);
  readonly catalog = inject(DashboardWidgetWizardCatalog);

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
    const item = this.data?.dashboardWidget;
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

    if (this.step() === 1 && this.state.selectedMetricDefinition() !== null) {
      this.step.set(2);
      return;
    }

    if (this.step() === 2 && this.state.selectedVisualizationType() !== null) {
      this.step.set(3);
      return;
    }

    if (this.step() === 3 && this.state.form.valid) {
      this.step.set(4);
    }
  }

  nextDisabled(): boolean {
    if (this.step() === 2) {
      return this.state.selectedVisualizationType() === null;
    }

    if (this.step() === 3) {
      return this.formInvalid() ?? true;
    }

    return true;
  }

  saveDashboardWidget(): void {
    const dataType = this.state.selectedMetricDefinition();
    const visualizationType = this.state.selectedVisualizationType();

    if (!dataType || !visualizationType || this.isSaving()) {
      return;
    }

    const visualizationTypeId = this.catalog.visualizationTypeId(visualizationType);
    const metricDefinitionId = this.catalog.metricDefinitionId(dataType);

    if (visualizationTypeId == null || metricDefinitionId == null) {
      this.saveError.set(
        'Visualization catalog is not available. Check that the API is running, then try again.'
      );
      return;
    }

    const value = this.state.form.getRawValue();
    const editingId = this.state.dashboardWidgetId();
    this.isSaving.set(true);
    this.saveError.set(null);

    this.dashboardWidgetService
      .upsertDashboardWidget({
        id: editingId ?? undefined,
        name: value.name.trim(),
        description: value.description.trim() || null,
        visualizationTypeId,
        metricDefinitionId,
        content: serializeDashboardWidgetContent(dataType, value)
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
    const id = this.state.dashboardWidgetId();
    if (id == null || this.isSaving()) {
      return;
    }

    confirmDeleteDashboardWidget(this.dialog).subscribe((confirmed) => {
      if (!confirmed || this.isSaving()) {
        return;
      }

      this.isSaving.set(true);
      this.dashboardWidgetService.deleteDashboardWidget(id).subscribe({
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
      ? 'Could not delete the widget. Please try again.'
      : this.state.isEditing()
        ? 'Could not update the widget. Please try again.'
        : 'Could not create the widget. Please try again.';
  }
}
