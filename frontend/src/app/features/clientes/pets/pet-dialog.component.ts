import { Component, Inject, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Pet, Sexo, SEXOS, sugestaoGramasDia } from './pet.model';

export interface PetDialogData {
  pet: Pet | null;
}

@Component({
  selector: 'app-pet-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
  ],
  templateUrl: './pet-dialog.component.html',
  styleUrl: './pet-dialog.component.scss',
})
export class PetDialogComponent {
  private readonly fb = inject(FormBuilder);

  readonly sexos = SEXOS;
  readonly edicao: boolean;

  readonly form = this.fb.nonNullable.group({
    nome: ['', [Validators.required]],
    raca: [''],
    pesoKg: [0, [Validators.required, Validators.min(0.1)]],
    dataNascimento: ['' as string | null],
    idadeAprox: [''],
    sexo: [null as Sexo | null],
    observacoesGerais: [''],
    observacoesAlimentares: [''],
    gramasDiaAjustadas: [null as number | null, [Validators.min(0)]],
  });

  constructor(
    private readonly ref: MatDialogRef<PetDialogComponent, Pet>,
    @Inject(MAT_DIALOG_DATA) readonly data: PetDialogData,
  ) {
    this.edicao = !!data.pet;
    if (data.pet) {
      const p = data.pet;
      this.form.patchValue({
        nome: p.nome,
        raca: p.raca,
        pesoKg: p.pesoKg,
        dataNascimento: p.dataNascimento ?? null,
        idadeAprox: p.idadeAprox ?? '',
        sexo: p.sexo,
        observacoesGerais: p.observacoesGerais,
        observacoesAlimentares: p.observacoesAlimentares,
        gramasDiaAjustadas: p.gramasDiaAjustadas,
      });
    }
  }

  get sugestao(): number | null {
    return sugestaoGramasDia(Number(this.form.getRawValue().pesoKg) || 0);
  }

  salvar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    const pet: Pet = {
      id: this.data.pet?.id ?? Date.now(),
      nome: v.nome.trim(),
      raca: v.raca.trim(),
      pesoKg: Number(v.pesoKg),
      dataNascimento: v.dataNascimento || null,
      idadeAprox: v.idadeAprox.trim() || null,
      sexo: v.sexo,
      ativo: this.data.pet?.ativo ?? true,
      observacoesGerais: v.observacoesGerais.trim(),
      observacoesAlimentares: v.observacoesAlimentares.trim(),
      gramasDiaAjustadas: v.gramasDiaAjustadas ?? null,
    };
    this.ref.close(pet);
  }

  cancelar(): void {
    this.ref.close();
  }
}
