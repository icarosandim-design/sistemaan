import { Injectable } from '@angular/core';
import { Observable, delay, of } from 'rxjs';
import { IngredienteAtivo, ReceitaCasa } from './receitas.model';

/**
 * Serviço da tela de Receitas da Casa.
 * NESTA FASE: dados mockados em memória (sem backend). Futuramente passará a
 * consumir os endpoints reais sem alterar a tela.
 */
@Injectable({ providedIn: 'root' })
export class ReceitasService {
  // Ingredientes ativos viriam do módulo Ingredientes (mock por ora).
  private readonly ingredientes: IngredienteAtivo[] = [
    { id: 1, nome: 'Frango (peito)', categoria: 'Proteína', coeficiente: 0.7, custoKg: 18.9 },
    { id: 2, nome: 'Carne bovina (patinho)', categoria: 'Proteína', coeficiente: 0.65, custoKg: 32.5 },
    { id: 3, nome: 'Carne suína', categoria: 'Proteína', coeficiente: 0.68, custoKg: 22.0 },
    { id: 4, nome: 'Fígado bovino', categoria: 'Proteína', coeficiente: 0.72, custoKg: 19.0 },
    { id: 5, nome: 'Arroz integral', categoria: 'Carboidrato', coeficiente: 3.0, custoKg: 7.2 },
    { id: 6, nome: 'Batata-doce', categoria: 'Carboidrato', coeficiente: 0.55, custoKg: 6.5 },
    { id: 7, nome: 'Abóbora', categoria: 'Vegetal', coeficiente: 0.8, custoKg: 4.8 },
    { id: 8, nome: 'Cenoura', categoria: 'Vegetal', coeficiente: 0.88, custoKg: 5.5 },
    { id: 9, nome: 'Óleo de coco', categoria: 'Óleo', coeficiente: 1, custoKg: 39.9 },
    { id: 10, nome: 'Suplemento vitamínico', categoria: 'Suplemento', coeficiente: 1, custoKg: 120.0 },
  ];

  private receitas: ReceitaCasa[] = [
    {
      id: 1,
      codigo: 'FRA',
      nome: 'Frango',
      ativo: true,
      observacoes: 'Cozinhar o frango desfiado. Misturar tudo ainda morno.',
      itens: [
        { ingredienteId: 1, gramas: 500 },
        { ingredienteId: 5, gramas: 300 },
        { ingredienteId: 8, gramas: 200 },
      ],
    },
    {
      id: 2,
      codigo: 'BOV',
      nome: 'Bovina',
      ativo: true,
      observacoes: 'Carne moída cozida sem temperos industrializados.',
      itens: [
        { ingredienteId: 2, gramas: 450 },
        { ingredienteId: 6, gramas: 350 },
        { ingredienteId: 7, gramas: 200 },
      ],
    },
    {
      id: 3,
      codigo: 'SUI',
      nome: 'Suína',
      ativo: false,
      observacoes: '',
      itens: [
        { ingredienteId: 3, gramas: 500 },
        { ingredienteId: 5, gramas: 300 },
        { ingredienteId: 8, gramas: 200 },
      ],
    },
  ];

  listarIngredientes(): Observable<IngredienteAtivo[]> {
    return of(this.ingredientes).pipe(delay(150));
  }

  listar(): Observable<ReceitaCasa[]> {
    return of(this.receitas.map((r) => ({ ...r, itens: [...r.itens] }))).pipe(delay(200));
  }

  salvar(receita: ReceitaCasa): Observable<ReceitaCasa> {
    if (receita.id) {
      this.receitas = this.receitas.map((r) => (r.id === receita.id ? receita : r));
    } else {
      receita = { ...receita, id: Date.now() };
      this.receitas = [receita, ...this.receitas];
    }
    return of(receita).pipe(delay(150));
  }

  alternarStatus(id: number): Observable<void> {
    this.receitas = this.receitas.map((r) => (r.id === id ? { ...r, ativo: !r.ativo } : r));
    return of(void 0).pipe(delay(150));
  }
}
