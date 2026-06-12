import { DatePipe } from '@angular/common';
import { Component, Inject, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { FrequenciaEntrega } from '../frequencias/frequencias.model';
import { FrequenciasService } from '../frequencias/frequencias.service';
import { MOTIVOS_REAGENDAMENTO } from './entregas.model';

export interface ReagendarDialogData {
  dataAtual: string;
}

export interface ReagendarDialogResult {
  escopo: 'pontual' | 'agenda';
  novaData: string;
  motivo: string;
  frequenciaEntregaId: number | null;
}

@Component({
  selector: 'app-reagendar-dialog',
  standalone: true,
  imports: [
    DatePipe,
    FormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatButtonToggleModule,
  ],
  template: `
    <h2 mat-dialog-title>Alterar data da entrega</h2>
    <mat-dialog-content>
      <p class="pergunta">Você quer alterar apenas esta entrega ou esta e as próximas?</p>
      <mat-button-toggle-group [(ngModel)]="escopo" class="escopo" hideSingleSelectionIndicator>
        <mat-button-toggle value="pontual">Somente esta entrega</mat-button-toggle>
        <mat-button-toggle value="agenda">Esta e próximas</mat-button-toggle>
      </mat-button-toggle-group>

      <p class="nota">
        @if (escopo === 'pontual') {
          A entrega de {{ data.dataAtual | date: 'dd/MM/yyyy' }} vira <strong>Reagendada</strong> e uma nova é criada na nova data. As próximas seguem o padrão atual.
        } @else {
          Atualiza a <strong>agenda do cliente</strong> (1ª data e, opcionalmente, frequência) e regera as próximas entregas elegíveis.
        }
      </p>

      <div class="form">
        <mat-form-field appearance="outline" class="full">
          <mat-label>Nova data</mat-label>
          <input matInput type="date" [(ngModel)]="novaData" />
        </mat-form-field>

        @if (escopo === 'agenda') {
          <mat-form-field appearance="outline" class="full">
            <mat-label>Frequência (opcional — manter atual se vazio)</mat-label>
            <mat-select [(ngModel)]="frequenciaEntregaId">
              <mat-option [value]="null">Manter atual</mat-option>
              @for (f of frequencias; track f.id) {
                <mat-option [value]="f.id">{{ f.nome }}</mat-option>
              }
            </mat-select>
          </mat-form-field>
        }

        <mat-form-field appearance="outline" class="full">
          <mat-label>Motivo</mat-label>
          <mat-select [(ngModel)]="motivoSel" (ngModelChange)="motivo = motivoSel === 'Outro' ? '' : motivoSel">
            @for (m of motivos; track m) {
              <mat-option [value]="m">{{ m }}</mat-option>
            }
          </mat-select>
        </mat-form-field>
        <mat-form-field appearance="outline" class="full">
          <mat-label>Detalhe / observação</mat-label>
          <textarea matInput rows="2" [(ngModel)]="motivo"></textarea>
        </mat-form-field>
      </div>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="ref.close()">Cancelar</button>
      <button mat-flat-button class="btn-cta" [disabled]="!novaData || !motivo.trim()" (click)="confirmar()">
        Confirmar
      </button>
    </mat-dialog-actions>
  `,
  styles: [
    `.pergunta { font-weight: 600; margin: 0 0 0.5rem; }
     .escopo { width: 100%; }
     .nota { font-size: 0.85rem; color: var(--an-texto-secundario); margin: 0.6rem 0; }
     .form { display: flex; flex-direction: column; gap: 0.25rem; min-width: 420px; }
     .full { width: 100%; }
     .btn-cta { background: var(--an-cta); color: #fff; }
     @media (max-width: 480px) { .form { min-width: auto; } }`,
  ],
})
export class ReagendarDialogComponent implements OnInit {
  private readonly frequenciasService = inject(FrequenciasService);

  readonly motivos = MOTIVOS_REAGENDAMENTO;
  frequencias: FrequenciaEntrega[] = [];

  escopo: 'pontual' | 'agenda' = 'pontual';
  novaData: string | null = null;
  motivoSel = '';
  motivo = '';
  frequenciaEntregaId: number | null = null;

  constructor(
    readonly ref: MatDialogRef<ReagendarDialogComponent, ReagendarDialogResult>,
    @Inject(MAT_DIALOG_DATA) readonly data: ReagendarDialogData,
  ) {}

  ngOnInit(): void {
    this.frequenciasService.listar().subscribe({
      next: (fs) => (this.frequencias = fs.filter((f) => f.ativo)),
      error: () => undefined,
    });
  }

  confirmar(): void {
    if (!this.novaData || !this.motivo.trim()) {
      return;
    }
    this.ref.close({
      escopo: this.escopo,
      novaData: this.novaData,
      motivo: this.motivo.trim(),
      frequenciaEntregaId: this.escopo === 'agenda' ? this.frequenciaEntregaId : null,
    });
  }
}
