import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';

export function openHowItWorksDialog(dialog: MatDialog): void {
  dialog.open(HowItWorksDialogComponent, {
    width: 'min(26.5rem, calc(100vw - 2rem))',
    maxHeight: 'min(90vh, 40rem)',
    autoFocus: 'first-tabbable',
    restoreFocus: true
  });
}

@Component({
  selector: 'app-how-it-works-dialog',
  imports: [MatButtonModule, MatDialogModule, MatIconModule],
  templateUrl: './how-it-works-dialog.component.html',
  styleUrl: './how-it-works-dialog.component.scss'
})
export class HowItWorksDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<HowItWorksDialogComponent>);

  close(): void {
    this.dialogRef.close();
  }
}
