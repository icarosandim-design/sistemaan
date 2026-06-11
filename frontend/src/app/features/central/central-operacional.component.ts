import { Component } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';

type StatusEstoque = 'ok' | 'baixo';
type Severidade = 'erro' | 'aviso' | 'info';

interface Ingrediente {
  nome: string;
  cozidos: number; // kg já cozidos/processados
  crus: number; // kg de cru necessário para a produção do dia
  estoque: number; // kg de cru disponível
}

@Component({
  selector: 'app-central-operacional',
  standalone: true,
  imports: [MatCardModule, MatIconModule, MatButtonModule, MatTooltipModule],
  templateUrl: './central-operacional.component.html',
  styleUrl: './central-operacional.component.scss',
})
export class CentralOperacionalComponent {
  // ===== DADOS FICTÍCIOS (placeholder) =====

  readonly diaSelecionado = '11/06';

  readonly kpis = [
    { label: 'Clientes ativos', valor: '150', icone: 'group' },
    { label: 'Pets', valor: '210', icone: 'pets' },
    { label: 'Produção / mês', valor: '3.600 kg', icone: 'scale' },
    { label: 'Recorrente / mês', valor: 'R$ 38.500', icone: 'payments' },
  ];

  /** Calendário apenas de ENTREGAS, próximos 7 dias. */
  readonly entregas7 = [
    { diaSemana: 'Qua', data: '11/06', entregas: 9, hoje: true },
    { diaSemana: 'Qui', data: '12/06', entregas: 5, hoje: false },
    { diaSemana: 'Sex', data: '13/06', entregas: 11, hoje: false },
    { diaSemana: 'Sáb', data: '14/06', entregas: 4, hoje: false },
    { diaSemana: 'Dom', data: '15/06', entregas: 0, hoje: false },
    { diaSemana: 'Seg', data: '16/06', entregas: 8, hoje: false },
    { diaSemana: 'Ter', data: '17/06', entregas: 6, hoje: false },
  ];

  /** Produção da casa (receitas padrão). */
  readonly producaoCasa = [
    { produto: 'Frango 250g', pacotes: 35 },
    { produto: 'Bovina 500g', pacotes: 10 },
    { produto: 'Suína 250g', pacotes: 22 },
  ];

  /** Receitas personalizadas (por pet). */
  readonly producaoPersonalizada = [
    { pet: 'Scooby', codigo: 'VET-001', tutor: 'Icaro', pacotes: 8 },
    { pet: 'Bidu', codigo: 'VET-002', tutor: 'Maria', pacotes: 7 },
  ];

  /** Ingredientes do dia: cozidos, crus necessários e estoque cru. */
  readonly ingredientes: Ingrediente[] = [
    { nome: 'Frango', cozidos: 1.0, crus: 12, estoque: 20 },
    { nome: 'Bovina', cozidos: 3.0, crus: 6, estoque: 4 },
    { nome: 'Suína', cozidos: 0.5, crus: 5, estoque: 9 },
    { nome: 'Arroz int.', cozidos: 0.6, crus: 5, estoque: 8 },
    { nome: 'Abóbora', cozidos: 0.8, crus: 3, estoque: 1.5 },
    { nome: 'Cenoura', cozidos: 0.4, crus: 2, estoque: 6 },
  ];

  /** Estoque de produto acabado das receitas da casa. */
  readonly estoque: { produto: string; saldo: number; minimo: number; status: StatusEstoque }[] = [
    { produto: 'Frango 250g', saldo: 120, minimo: 80, status: 'ok' },
    { produto: 'Bovina 250g', saldo: 45, minimo: 60, status: 'baixo' },
    { produto: 'Frango 500g', saldo: 30, minimo: 20, status: 'ok' },
    { produto: 'Suína 250g', saldo: 18, minimo: 40, status: 'baixo' },
    { produto: 'Peixe 250g', saldo: 64, minimo: 30, status: 'ok' },
  ];

  readonly alertas: { tipo: Severidade; icone: string; texto: string }[] = [
    { tipo: 'erro', icone: 'inventory_2', texto: 'Carne bovina e abóbora abaixo do necessário para hoje' },
    { tipo: 'aviso', icone: 'pending_actions', texto: '2 receitas personalizadas aguardando produção' },
    { tipo: 'info', icone: 'local_shipping', texto: '9 entregas previstas para hoje' },
  ];

  readonly acoes = [
    { label: 'Novo cliente', icone: 'person_add' },
    { label: 'Planejar produção', icone: 'factory' },
  ];

  // ===== Derivados =====

  get subtotalCasa(): number {
    return this.producaoCasa.reduce((s, c) => s + c.pacotes, 0);
  }

  get subtotalPersonalizada(): number {
    return this.producaoPersonalizada.reduce((s, p) => s + p.pacotes, 0);
  }

  get totalPacotes(): number {
    return this.subtotalCasa + this.subtotalPersonalizada;
  }

  get totalReceitas(): number {
    return this.producaoCasa.length + this.producaoPersonalizada.length;
  }

  get faltantes(): { nome: string; falta: number }[] {
    return this.ingredientes
      .filter((i) => i.estoque < i.crus)
      .map((i) => ({ nome: i.nome, falta: i.crus - i.estoque }));
  }

  get bannerTexto(): string {
    const partes = this.faltantes.map((f) => `${this.fmtNum(f.falta)} kg de ${f.nome.toLowerCase()}`);
    return `Comprar ${this.juntar(partes)}.`;
  }

  fmtKg(v: number): string {
    return `${this.fmtNum(v)}kg`;
  }

  private fmtNum(v: number): string {
    return v.toLocaleString('pt-BR', { maximumFractionDigits: 1 });
  }

  private juntar(itens: string[]): string {
    if (itens.length <= 1) {
      return itens.join('');
    }
    return `${itens.slice(0, -1).join(', ')} e ${itens[itens.length - 1]}`;
  }
}
