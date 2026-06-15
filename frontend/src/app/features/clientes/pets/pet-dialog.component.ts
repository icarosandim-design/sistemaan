import { Component, Inject, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { catchError, debounceTime, distinctUntilChanged, of, switchMap } from 'rxjs';
import { Pet, PetReceitaPronta, SalvarPetRequest, Sexo, SEXOS } from './pet.model';
import { PetService } from './pet.service';

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
  private readonly petService = inject(PetService);

  readonly sexos = SEXOS;
  readonly edicao: boolean;
  readonly sugestao = signal<number | null>(null);
  readonly prontos = signal<PetReceitaPronta[]>([]);

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
    private readonly ref: MatDialogRef<PetDialogComponent, SalvarPetRequest>,
    @Inject(MAT_DIALOG_DATA) readonly data: PetDialogData,
  ) {
    this.edicao = !!data.pet;
    if (data.pet) {
      const p = data.pet;
      this.form.patchValue({
        nome: p.nome,
        raca: p.raca ?? '',
        pesoKg: p.pesoKg,
        dataNascimento: p.dataNascimento ?? null,
        idadeAprox: p.idadeAprox ?? '',
        sexo: p.sexo,
        observacoesGerais: p.observacoesGerais ?? '',
        observacoesAlimentares: p.observacoesAlimentares ?? '',
        gramasDiaAjustadas: p.gramasDiaAjustadas,
      });
      this.sugestao.set(p.gramasDiaSugeridas);
      this.petService.prontos(p.id).subscribe({ next: (r) => this.prontos.set(r), error: () => {} });
    }

    // Sugestão ao vivo a partir da Tabela de Consumo (backend).
    this.form.controls.pesoKg.valueChanges
      .pipe(
        debounceTime(350),
        distinctUntilChanged(),
        switchMap((peso) => {
          const v = Number(peso) || 0;
          return v > 0 ? this.petService.sugestaoPorPeso(v).pipe(catchError(() => of(null))) : of(null);
        }),
        takeUntilDestroyed(),
      )
      .subscribe((g) => this.sugestao.set(g));
  }

  salvar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    const txt = (s: string) => (s.trim() ? s.trim() : null);
    const req: SalvarPetRequest = {
      nome: v.nome.trim(),
      raca: txt(v.raca),
      pesoKg: Number(v.pesoKg),
      dataNascimento: v.dataNascimento || null,
      idadeAprox: txt(v.idadeAprox),
      sexo: v.sexo,
      observacoesGerais: txt(v.observacoesGerais),
      observacoesAlimentares: txt(v.observacoesAlimentares),
      gramasDiaAjustadas: v.gramasDiaAjustadas ?? null,
    };
    this.ref.close(req);
  }

  cancelar(): void {
    this.ref.close();
  }
}
