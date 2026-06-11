import { Component } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';

type StatusEstoque = 'ok' | 'baixo';
type TipoProducao = 'casa' | 'personalizada';
type Severidade = 'erro' | 'aviso' | 'info';

@Component({
  selector: 'app-central-operacional',
  standalone: true,
  imports: [MatCardModule, MatIconModule, MatButtonModule, MatTooltipModule],
  templateUrl: './central-operacional.component.html',
  styleUrl: './central-operacional.component.scss',
})
export class CentralOperacionalComponent {
  // ===== DADOS FICTÍCIOS (placeholder) =====
  // Serão substituídos por dados reais conforme os módulos forem implementados.

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

  /** O que será cozinhado no dia + para quem. */
  readonly cozinharHoje: { item: string; qtd: string; tipo: TipoProducao; para: string | null }[] = [
    { item: 'Frango 250g', qtd: '35 pacotes', tipo: 'casa', para: null },
    { item: 'Bovina 500g', qtd: '10 pacotes', tipo: 'casa', para: null },
    { item: 'Suína 250g', qtd: '22 pacotes', tipo: 'casa', para: null },
    { item: 'VET-001', qtd: '8 pacotes', tipo: 'personalizada', para: 'Icaro · Scooby' },
    { item: 'VET-002', qtd: '7 pacotes', tipo: 'personalizada', para: 'Maria · Bidu' },
  ];

  /** Ingredientes crus necessários para a produção do dia × estoque cru disponível. */
  readonly ingredientesCrus: { nome: string; necessario: string; estoque: string; status: StatusEstoque }[] = [
    { nome: 'Frango (peito)', necessario: '12 kg', estoque: '20 kg', status: 'ok' },
    { nome: 'Carne bovina', necessario: '6 kg', estoque: '4 kg', status: 'baixo' },
    { nome: 'Carne suína', necessario: '5 kg', estoque: '9 kg', status: 'ok' },
    { nome: 'Arroz integral', necessario: '5 kg', estoque: '8 kg', status: 'ok' },
    { nome: 'Abóbora', necessario: '3 kg', estoque: '1,5 kg', status: 'baixo' },
    { nome: 'Cenoura', necessario: '2 kg', estoque: '6 kg', status: 'ok' },
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
}
