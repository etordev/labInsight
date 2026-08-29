import { Component, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { GraphWizardState } from './graph-wizard-state';
import { SelectDataStepComponent } from './select-data-step/select-data-step.component';

@Component({
  selector: 'app-graph-wizard-dialog',
  imports: [MatButtonModule, MatDialogModule, SelectDataStepComponent],
  providers: [GraphWizardState],
  templateUrl: './graph-wizard-dialog.component.html',
  styleUrl: './graph-wizard-dialog.component.scss'
})
export class GraphWizardDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<GraphWizardDialogComponent>);
  readonly state = inject(GraphWizardState);

  readonly step = signal<1 | 2>(1);

  close(): void {
    this.dialogRef.close();
  }

  goBack(): void {
    this.step.set(1);
  }

  continue(): void {
    if (this.step() === 1 && this.state.selectedGraphDataType() !== null) {
      this.step.set(2);
    }
  }
}
