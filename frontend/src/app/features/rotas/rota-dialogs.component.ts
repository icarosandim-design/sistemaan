import { Component, Inject, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialog, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CdkDragDrop, DragDropModule, moveItemInArray } from '@angular/cdk/drag-drop';
import {
  EntregaDisponivel, fmtKg, PERIODOS_ROTA, preferenciaIncompativel, Rota, rotuloPeriodo,
  rotuloStatusRota, StatusRota,
} from './rotas.model';
import { RotasService } from './rotas.service';
import { rotuloPreferenciaHorario } from '../clientes/clientes.model';

function msgErro(e: unknown): string {
  const err = (e as { error?: { errors?: Record<string, string[]>; detail?: string } })?.error;
  const first = err?.errors ? Object.values(err.errors)[0]?.[0] : undefined;
  return first ?? err?.detail ?? 'Não foi possível concluir a operação.';
}

// ===================== Form: criar/editar saída =====================
@Component({
  selector: 'app-rota-form-dialog',
  standalone: true,
  imports: [FormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatButtonModule, MatIconModule],
  template: `
    <h2 mat-dialog-title>{{ data.rota ? 'Editar saída' : 'Nova saída' }}</h2>
    <mat-dialog-content>
      <mat-form-field appearance="outline" class="full">
        <mat-label>Nome da saída</mat-label>
        <input matInput [(ngModel)]="nome" placeholder="Ex.: Manhã — Zona Oeste" />
      </mat-form-field>
      <mat-form-field appearance="outline" class="full">
        <mat-label>Período</mat-label>
        <mat-select [(ngModel)]="periodo">
          @for (p of periodos; track p.valor) { <mat-option [value]="p.valor">{{ p.rotulo }}</mat-option> }
        </mat-select>
      </mat-form-field>
      <mat-form-field appearance="outline" class="full">
        <mat-label>Entregador</mat-label>
        <input matInput [(ngModel)]="entregador" />
      </mat-form-field>
      <mat-form-field appearance="outline" class="full">
        <mat-label>Observações</mat-label>
        <textarea matInput rows="2" [(ngModel)]="observacoes"></textarea>
      </mat-form-field>
      @if (erro()) { <p class="erro"><mat-icon>error</mat-icon> {{ erro() }}</p> }
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="ref.close()">Cancelar</button>
      <button mat-flat-button class="btn-cta" [disabled]="salvando()" (click)="salvar()"><mat-icon>check</mat-icon> Salvar</button>
    </mat-dialog-actions>
  `,
  styles: [`
    .full { width: 100%; }
    .erro { display: flex; align-items: center; gap: 0.4rem; color: #b3261e; font-size: 0.85rem; margin: 0; .mat-icon { font-size: 1.05rem; width: 1.05rem; height: 1.05rem; } }
    .btn-cta { background: var(--an-cta); color: #fff; }
    mat-dialog-content { min-width: 420px; display: flex; flex-direction: column; }
    @media (max-width: 460px) { mat-dialog-content { min-width: auto; } }
  `],
})
export class RotaFormDialogComponent {
  private readonly service = inject(RotasService);
  readonly periodos = PERIODOS_ROTA;
  nome: string;
  periodo: string;
  entregador: string;
  observacoes: string;
  readonly salvando = signal(false);
  readonly erro = signal<string | null>(null);

  constructor(
    readonly ref: MatDialogRef<RotaFormDialogComponent, Rota>,
    @Inject(MAT_DIALOG_DATA) readonly data: { data: string; rota?: Rota },
  ) {
    this.nome = data.rota?.nome ?? '';
    this.periodo = data.rota?.periodo ?? 'HorarioComercial';
    this.entregador = data.rota?.entregador ?? '';
    this.observacoes = data.rota?.observacoes ?? '';
  }

  salvar(): void {
    if (!this.nome.trim()) { this.erro.set('Informe o nome da saída.'); return; }
    this.salvando.set(true);
    this.erro.set(null);
    const base = { nome: this.nome.trim(), periodo: this.periodo, entregador: this.entregador.trim() || null, observacoes: this.observacoes.trim() || null };
    const obs = this.data.rota
      ? this.service.atualizar(this.data.rota.id, base)
      : this.service.criar({ data: this.data.data, ...base });
    obs.subscribe({
      next: (r) => this.ref.close(r),
      error: (e) => { this.salvando.set(false); this.erro.set(msgErro(e)); },
    });
  }
}

// ===================== Detalhe: gerenciar a saída =====================
@Component({
  selector: 'app-rota-detalhe-dialog',
  standalone: true,
  imports: [MatDialogModule, MatButtonModule, MatIconModule, MatTooltipModule, MatProgressSpinnerModule, DragDropModule],
  templateUrl: './rota-detalhe-dialog.component.html',
  styleUrl: './rota-detalhe-dialog.component.scss',
})
export class RotaDetalheDialogComponent implements OnInit {
  private readonly service = inject(RotasService);
  private readonly dialog = inject(MatDialog);
  private readonly snack = inject(MatSnackBar);

  readonly rota = signal<Rota | null>(null);
  readonly disponiveis = signal<EntregaDisponivel[]>([]);
  readonly carregando = signal(true);
  alterou = false;

  readonly rotuloPeriodo = rotuloPeriodo;
  readonly rotuloStatusRota = rotuloStatusRota;
  readonly rotuloPreferencia = rotuloPreferenciaHorario;
  readonly preferenciaIncompativel = preferenciaIncompativel;
  readonly fmtKg = fmtKg;

  /** Peso total da carga da saída (soma das paradas). */
  get cargaTotalGramas(): number {
    return (this.rota()?.paradas ?? []).reduce((s, p) => s + p.totalGramas, 0);
  }

  constructor(
    readonly ref: MatDialogRef<RotaDetalheDialogComponent, boolean>,
    @Inject(MAT_DIALOG_DATA) readonly data: { rotaId: number },
  ) {}

  ngOnInit(): void {
    this.carregar();
  }

  private carregar(): void {
    this.carregando.set(true);
    this.service.obter(this.data.rotaId).subscribe({
      next: (r) => {
        this.rota.set(r);
        this.carregando.set(false);
        this.service.disponiveis(r.data).subscribe((d) => this.disponiveis.set(d));
      },
      error: () => this.carregando.set(false),
    });
  }

  fmtData(iso: string): string {
    const [y, m, d] = iso.split('-');
    return `${d}/${m}/${y}`;
  }

  get ativa(): boolean {
    const s = this.rota()?.status;
    return s === 'Rascunho' || s === 'Planejada' || s === 'Despachada';
  }

  adicionar(e: EntregaDisponivel): void {
    this.service.adicionarEntrega(this.data.rotaId, e.id).subscribe({
      next: (r) => { this.alterou = true; this.rota.set(r); this.recarregarDisponiveis(); },
      error: (err) => this.snack.open(msgErro(err), 'OK', { duration: 4000 }),
    });
  }

  remover(entregaId: number): void {
    this.service.removerEntrega(this.data.rotaId, entregaId).subscribe({
      next: (r) => { this.alterou = true; this.rota.set(r); this.recarregarDisponiveis(); },
      error: (err) => this.snack.open(msgErro(err), 'OK', { duration: 4000 }),
    });
  }

  aoArrastar(event: CdkDragDrop<unknown>): void {
    const r = this.rota();
    if (!r || event.previousIndex === event.currentIndex) {
      return;
    }
    const ids = r.paradas.map((p) => p.entregaId);
    moveItemInArray(ids, event.previousIndex, event.currentIndex);
    this.service.reordenar(this.data.rotaId, ids).subscribe({
      next: (res) => { this.alterou = true; this.rota.set(res); },
      error: (err) => this.snack.open(msgErro(err), 'OK', { duration: 3000 }),
    });
  }

  mover(index: number, delta: number): void {
    const r = this.rota();
    if (!r) return;
    const ids = r.paradas.map((p) => p.entregaId);
    const novo = index + delta;
    if (novo < 0 || novo >= ids.length) return;
    [ids[index], ids[novo]] = [ids[novo], ids[index]];
    this.service.reordenar(this.data.rotaId, ids).subscribe({
      next: (res) => { this.alterou = true; this.rota.set(res); },
      error: (err) => this.snack.open(msgErro(err), 'OK', { duration: 3000 }),
    });
  }

  mudarStatus(status: StatusRota): void {
    if (status === 'Concluida') {
      const pendentes = (this.rota()?.paradas ?? []).filter((p) => p.statusEntrega !== 'Entregue').length;
      if (pendentes > 0) {
        this.snack.open(`Atenção: ${pendentes} entrega(s) ainda não estão como Entregue.`, 'OK', { duration: 4000 });
      }
    }
    this.service.mudarStatus(this.data.rotaId, status).subscribe({
      next: (r) => {
        this.alterou = true;
        this.rota.set(r);
        if (status === 'Despachada') this.snack.open('Rota despachada — entregas marcadas como "Saiu para entrega".', 'OK', { duration: 3500 });
      },
      error: (err) => this.snack.open(msgErro(err), 'OK', { duration: 4000 }),
    });
  }

  editar(): void {
    const r = this.rota();
    if (!r) return;
    const ref = this.dialog.open(RotaFormDialogComponent, { data: { data: r.data, rota: r }, autoFocus: false });
    ref.afterClosed().subscribe((res) => { if (res) { this.alterou = true; this.rota.set(res); } });
  }

  imprimir(): void {
    window.open(`/rotas/${this.data.rotaId}/impressao`, '_blank');
  }

  copiar(): void {
    const r = this.rota();
    if (!r) return;
    const linhas: string[] = [
      `Rota — ${this.fmtData(r.data)}`,
      `Saída: ${r.nome} (${rotuloPeriodo(r.periodo)})`,
      `Entregador: ${r.entregador || '—'}`,
      '',
    ];
    linhas.push(`Carga total: ${this.fmtKg(this.cargaTotalGramas)}`, '');
    r.paradas.forEach((p, i) => {
      linhas.push(`${i + 1}. ${p.clienteNome}${p.ehPj ? ' [PJ]' : ''} — ${this.rotuloPreferencia(p.preferenciaHorario)}`);
      if (p.petNomes) linhas.push(`   Cão: ${p.petNomes}`);
      linhas.push(`   ${p.endereco}${p.bairro ? ' — ' + p.bairro : ''}${p.cidade ? ', ' + p.cidade : ''}`);
      if (p.itensResumo) linhas.push(`   Itens: ${p.itensResumo} (${this.fmtKg(p.totalGramas)})`);
      linhas.push('');
    });
    navigator.clipboard.writeText(linhas.join('\n')).then(
      () => this.snack.open('Rota copiada para a área de transferência.', 'OK', { duration: 2500 }),
      () => this.snack.open('Não foi possível copiar.', 'OK', { duration: 2500 }),
    );
  }

  /** Gera uma imagem (JPEG) da rota para enviar ao entregador (sem telefone do cliente). */
  baixarImagem(): void {
    const r = this.rota();
    if (!r) {
      return;
    }
    const linhas: { txt: string; bold?: boolean; small?: boolean; gap?: boolean }[] = [];
    linhas.push({ txt: `Rota — ${this.fmtData(r.data)}`, bold: true });
    linhas.push({ txt: `${r.nome} · ${this.rotuloPeriodo(r.periodo)} · Entregador: ${r.entregador || '—'}`, small: true });
    linhas.push({ txt: `Carga total: ${this.fmtKg(this.cargaTotalGramas)}`, small: true });
    linhas.push({ txt: '', gap: true });
    r.paradas.forEach((p, i) => {
      linhas.push({ txt: `${i + 1}. ${p.clienteNome}${p.ehPj ? ' [PJ]' : ''}`, bold: true });
      if (p.petNomes) {
        linhas.push({ txt: `    Cão: ${p.petNomes}`, small: true });
      }
      linhas.push({ txt: `    ${p.endereco}${p.bairro ? ' — ' + p.bairro : ''}${p.cidade ? ', ' + p.cidade : ''}`, small: true });
      if (p.itensResumo) {
        linhas.push({ txt: `    ${p.itensResumo}`, small: true });
      }
      linhas.push({ txt: `    ${this.rotuloPreferencia(p.preferenciaHorario)} · ${this.fmtKg(p.totalGramas)}`, small: true });
      linhas.push({ txt: '', gap: true });
    });

    const W = 720;
    const pad = 24;
    const lh = 22;
    const altura = pad * 2 + linhas.reduce((s, l) => s + (l.gap ? lh / 2 : lh), 0);
    const canvas = document.createElement('canvas');
    canvas.width = W;
    canvas.height = Math.max(altura, 200);
    const ctx = canvas.getContext('2d');
    if (!ctx) {
      return;
    }
    ctx.fillStyle = '#ffffff';
    ctx.fillRect(0, 0, canvas.width, canvas.height);
    ctx.textBaseline = 'top';
    let y = pad;
    for (const l of linhas) {
      if (l.gap) { y += lh / 2; continue; }
      ctx.font = `${l.bold ? '700 ' : ''}${l.small ? 14 : 18}px Arial, sans-serif`;
      ctx.fillStyle = l.bold ? '#3f4f2d' : '#333333';
      ctx.fillText(l.txt, pad, y);
      y += lh;
    }
    const a = document.createElement('a');
    a.href = canvas.toDataURL('image/jpeg', 0.92);
    a.download = `rota-${r.data}-${r.nome.replace(/\s+/g, '_')}.jpg`;
    a.click();
    this.snack.open('Imagem da rota baixada (JPEG).', 'OK', { duration: 2500 });
  }

  private recarregarDisponiveis(): void {
    const r = this.rota();
    if (r) this.service.disponiveis(r.data).subscribe((d) => this.disponiveis.set(d));
  }
}
