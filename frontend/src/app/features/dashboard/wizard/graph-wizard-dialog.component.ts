import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, OnInit, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { map, startWith } from 'rxjs';
import { serializeGraphItemContent } from '../models/build-graph-item-content';
import { GraphItemService } from '../services/graph-item.service';
import { ConfigureGraphStepComponent } from './configure-graph-step/configure-graph-step.component';
import { GraphWizardCatalog } from './graph-wizard-catalog';
import { GraphWizardState } from './graph-wizard-state';
import { ReviewGraphStepComponent } from './review-graph-step/review-graph-step.component';
import { SelectDataStepComponent } from './select-data-step/select-data-step.component';
import { SelectGraphTypeStepComponent } from './select-graph-type-step/select-graph-type-step.component';

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
  private readonly dialogRef = inject(MatDialogRef<GraphWizardDialogComponent>);
  private readonly graphItemService = inject(GraphItemService);
  readonly state = inject(GraphWizardState);
  readonly catalog = inject(GraphWizardCatalog);

  readonly step = signal<1 | 2 | 3 | 4>(1);
  readonly isCreating = signal(false);
  readonly createError = signal<string | null>(null);

  readonly formInvalid = toSignal(
    this.state.form.statusChanges.pipe(
      startWith(this.state.form.status),
      map(() => this.state.form.invalid)
    ),
    { initialValue: this.state.form.invalid }
  );

  ngOnInit(): void {
    this.catalog.load();
  }

  close(): void {
    this.dialogRef.close();
  }

  goBack(): void {
    this.createError.set(null);
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
    this.createError.set(null);

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

  createGraph(): void {
    const dataType = this.state.selectedGraphDataType();
    const graphType = this.state.selectedGraphType();

    if (!dataType || !graphType || this.isCreating()) {
      return;
    }

    const graphTypeId = this.catalog.graphTypeId(graphType);
    const graphDataTypeId = this.catalog.graphDataTypeId(dataType);

    if (graphTypeId == null || graphDataTypeId == null) {
      this.createError.set(
        'Graph metadata is not available. Check that the API is running, then try again.'
      );
      return;
    }

    const value = this.state.form.getRawValue();
    this.isCreating.set(true);
    this.createError.set(null);

    this.graphItemService
      .createGraphItem({
        name: value.name.trim(),
        description: value.description.trim() || null,
        graphTypeId,
        graphDataTypeId,
        content: serializeGraphItemContent(dataType, value)
      })
      .subscribe({
        next: (item) => this.dialogRef.close(item),
        error: (error: unknown) => {
          this.isCreating.set(false);
          this.createError.set(this.readErrorMessage(error));
        }
      });
  }

  private readErrorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      const payload = error.error as { message?: string } | null;
      if (payload && typeof payload.message === 'string' && payload.message.length > 0) {
        return payload.message;
      }

      if (error.status === 0) {
        return 'Unable to reach the API. Check that it is running, then try again.';
      }
    }

    return 'Could not create the graph. Please try again.';
  }
}
