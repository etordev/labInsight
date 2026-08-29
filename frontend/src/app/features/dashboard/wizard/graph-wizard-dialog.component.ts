import { Component, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { GraphWizardState } from './graph-wizard-state';
import { SelectDataStepComponent } from './select-data-step/select-data-step.component';
import { SelectGraphTypeStepComponent } from './select-graph-type-step/select-graph-type-step.component';

@Component({
  selector: 'app-graph-wizard-dialog',
  imports: [
    MatButtonModule,
    MatDialogModule,
    SelectDataStepComponent,
    SelectGraphTypeStepComponent
  ],
  providers: [GraphWizardState],
  templateUrl: './graph-wizard-dialog.component.html',
  styleUrl: './graph-wizard-dialog.component.scss'
})
export class GraphWizardDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<GraphWizardDialogComponent>);
  readonly state = inject(GraphWizardState);

  readonly step = signal<1 | 2 | 3>(1);

  close(): void {
    this.dialogRef.close();
  }

  goBack(): void {
    if (this.step() === 3) {
      this.step.set(2);
      return;
    }

    this.step.set(1);
  }

  continue(): void {
    if (this.step() === 1 && this.state.selectedGraphDataType() !== null) {
      this.step.set(2);
      return;
    }

    if (this.step() === 2 && this.state.selectedGraphType() !== null) {
      this.step.set(3);
    }
  }
}
