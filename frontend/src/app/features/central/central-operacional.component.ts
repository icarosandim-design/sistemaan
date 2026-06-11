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

  readonly kpis = [
    { label: 'Clientes ativos', valor: '150', icone: 'group' },
    { label: 'Pets', valor: '210', icone: 'pets' },
    { label: 'Produção / mês', valor: '3.600 kg', icone: 'scale' },
    { label: 'Recorrente / mês', valor: 'R$ 38.500', icone: 'payments' },
  ];

  readonly dias7 = [
    { diaSemana: 'Seg', data: '09/06', entregas: 8, producao: 2, hoje: true },
    { diaSemana: 'Ter', data: '10/06', entregas: 6, producao: 1, hoje: false },
    { diaSemana: 'Qua', data: '11/06', entregas: 9, producao: 3, hoje: false },
    { diaSemana: 'Qui', data: '12/06', entregas: 5, producao: 0, hoje: false },
    { diaSemana: 'Sex', data: '13/06', entregas: 11, producao: 2, hoje: false },
    { diaSemana: 'Sáb', data: '14/06', entregas: 4, producao: 0, hoje: false },
    { diaSemana: 'Dom', data: '15/06', entregas: 0, producao: 0, hoje: false },
  ];

  readonly estoque: { produto: string; saldo: number; minimo: number; status: StatusEstoque }[] = [
    { produto: 'Frango 250g', saldo: 120, minimo: 80, status: 'ok' },
    { produto: 'Bovina 250g', saldo: 45, minimo: 60, status: 'baixo' },
    { produto: 'Frango 500g', saldo: 30, minimo: 20, status: 'ok' },
    { produto: 'Suína 250g', saldo: 18, minimo: 40, status: 'baixo' },
    { produto: 'Peixe 250g', saldo: 64, minimo: 30, status: 'ok' },
  ];

  readonly producaoPendentes = 5;

  readonly producao: { item: string; qtd: string; tipo: TipoProducao }[] = [
    { item: 'Frango 250g', qtd: '+35 pacotes', tipo: 'casa' },
    { item: 'Bovina 250g', qtd: '+20 pacotes', tipo: 'casa' },
    { item: 'Suína 250g', qtd: '+22 pacotes', tipo: 'casa' },
    { item: 'VET-001', qtd: '+8 pacotes', tipo: 'personalizada' },
    { item: 'VET-002', qtd: '+4 pacotes', tipo: 'personalizada' },
  ];

  readonly alertas: { tipo: Severidade; icone: string; texto: string }[] = [
    { tipo: 'erro', icone: 'inventory_2', texto: 'Bovina 250g e Suína 250g abaixo do estoque mínimo' },
    { tipo: 'aviso', icone: 'pending_actions', texto: '2 receitas personalizadas aguardando produção' },
    { tipo: 'info', icone: 'local_shipping', texto: '8 entregas previstas para hoje' },
  ];

  readonly acoes = [
    { label: 'Novo cliente', icone: 'person_add' },
    { label: 'Novo pet', icone: 'pets' },
    { label: 'Registrar produção', icone: 'factory' },
    { label: 'Nova entrega', icone: 'local_shipping' },
  ];
}
