import { Injectable } from '@angular/core';
import { Observable, delay, of } from 'rxjs';
import { CentralResumo } from './central.model';

/**
 * Fornece o resumo da Central Operacional.
 *
 * NESTA FASE: retorna dados mockados (contrato visual estruturado).
 * FUTURO: substituir a implementação por uma chamada HTTP a
 *   `GET /api/central/resumo`
 * sem alterar a tela — o componente já depende apenas deste contrato.
 *
 * A Central NÃO contém regra de negócio: ela apenas lê/agrega o que os módulos
 * donos (Clientes, Pets, Produção, Estoque, Entregas, Financeiro) calcularão.
 */
@Injectable({ providedIn: 'root' })
export class CentralService {
  obterResumo(): Observable<CentralResumo> {
    // TODO(integração): trocar por `this.http.get<CentralResumo>('/api/central/resumo')`
    // preenchendo os campos incrementalmente conforme cada módulo ficar pronto.
    return of(MOCK_RESUMO).pipe(delay(300));
  }
}

const MOCK_RESUMO: CentralResumo = {
  diaSelecionado: '11/06',
  kpis: [
    { label: 'Clientes ativos', valor: '150', icone: 'group' },
    { label: 'Pets', valor: '210', icone: 'pets' },
    { label: 'Produção / mês', valor: '3.600 kg', icone: 'scale' },
    { label: 'Recorrente / mês', valor: 'R$ 38.500', icone: 'payments' },
  ],
  entregas7: [
    { diaSemana: 'Qua', data: '11/06', entregas: 9, hoje: true },
    { diaSemana: 'Qui', data: '12/06', entregas: 5, hoje: false },
    { diaSemana: 'Sex', data: '13/06', entregas: 11, hoje: false },
    { diaSemana: 'Sáb', data: '14/06', entregas: 4, hoje: false },
    { diaSemana: 'Dom', data: '15/06', entregas: 0, hoje: false },
    { diaSemana: 'Seg', data: '16/06', entregas: 8, hoje: false },
    { diaSemana: 'Ter', data: '17/06', entregas: 6, hoje: false },
  ],
  producaoCasa: [
    { produto: 'Frango 250g', pacotes: 35 },
    { produto: 'Frango 500g', pacotes: 12 },
    { produto: 'Bovina 250g', pacotes: 18 },
    { produto: 'Bovina 500g', pacotes: 10 },
    { produto: 'Suína 250g', pacotes: 22 },
    { produto: 'Suína 500g', pacotes: 8 },
  ],
  producaoPersonalizada: [
    { pet: 'Scooby', codigo: 'VET-001', tutor: 'Icaro', pacotes: 8 },
    { pet: 'Bidu', codigo: 'VET-002', tutor: 'Maria', pacotes: 7 },
    { pet: 'Rex', codigo: 'VET-003', tutor: 'João', pacotes: 6 },
    { pet: 'Mel', codigo: 'VET-004', tutor: 'Ana', pacotes: 5 },
    { pet: 'Thor', codigo: 'VET-005', tutor: 'Carlos', pacotes: 9 },
    { pet: 'Luna', codigo: 'VET-006', tutor: 'Paula', pacotes: 4 },
    { pet: 'Nina', codigo: 'VET-007', tutor: 'Bruno', pacotes: 6 },
    { pet: 'Bob', codigo: 'VET-008', tutor: 'Carla', pacotes: 5 },
  ],
  ingredientes: [
    { nome: 'Frango', cozidos: 1.0, crus: 12, estoque: 20 },
    { nome: 'Bovina', cozidos: 3.0, crus: 6, estoque: 4 },
    { nome: 'Suína', cozidos: 0.5, crus: 5, estoque: 9 },
    { nome: 'Arroz int.', cozidos: 0.6, crus: 5, estoque: 8 },
    { nome: 'Abóbora', cozidos: 0.8, crus: 3, estoque: 1.5 },
    { nome: 'Cenoura', cozidos: 0.4, crus: 2, estoque: 6 },
  ],
  estoque: [
    { produto: 'Frango 250g', saldo: 120, minimo: 80, status: 'ok' },
    { produto: 'Bovina 250g', saldo: 45, minimo: 60, status: 'baixo' },
    { produto: 'Frango 500g', saldo: 30, minimo: 20, status: 'ok' },
    { produto: 'Suína 250g', saldo: 18, minimo: 40, status: 'baixo' },
    { produto: 'Peixe 250g', saldo: 64, minimo: 30, status: 'ok' },
  ],
  alertas: [
    { tipo: 'erro', icone: 'inventory_2', texto: 'Carne bovina e abóbora abaixo do necessário para hoje' },
    { tipo: 'aviso', icone: 'pending_actions', texto: '2 receitas personalizadas aguardando produção' },
    { tipo: 'info', icone: 'local_shipping', texto: '9 entregas previstas para hoje' },
  ],
};
