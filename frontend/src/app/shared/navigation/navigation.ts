import { Component, OnInit } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatBadgeModule } from '@angular/material/badge';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDividerModule } from '@angular/material/divider';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../auth/auth.service';
import { filter } from 'rxjs/operators';

interface NavigationItem {
  label: string;
  route: string;
  icon: string;
  description: string;
  badge?: number;
  children?: NavigationItem[];
}

@Component({
  selector: 'app-navigation',
  standalone: true,
  imports: [
    CommonModule,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    MatBadgeModule,
    MatTooltipModule,
    MatDividerModule,
    RouterLink,
    RouterLinkActive
  ],
  templateUrl: './navigation.html',
  styleUrls: ['./navigation.scss']
})
export class NavigationComponent implements OnInit {
  currentRoute = '';
  user: any = null;

  navigationItems: NavigationItem[] = [
    {
      label: 'Dashboard',
      route: '/dashboard',
      icon: 'dashboard',
      description: 'Visão geral das suas finanças'
    },
    {
      label: 'Contas',
      route: '/contas',
      icon: 'account_balance',
      description: 'Gerenciar contas bancárias e cartões'
    },
    {
      label: 'Categorias',
      route: '/categorias',
      icon: 'category',
      description: 'Organizar gastos por categoria'
    },
    {
      label: 'Lançamentos',
      route: '/lancamentos',
      icon: 'receipt_long',
      description: 'Registrar receitas e despesas'
    },
    {
      label: 'Folha Mensal',
      route: '/folha-mensal',
      icon: 'calendar_month',
      description: 'Controle mensal detalhado'
    },
    {
      label: 'Relatórios',
      route: '/relatorios',
      icon: 'analytics',
      description: 'Análises e gráficos financeiros',
      children: [
        {
          label: 'Por Categoria',
          route: '/relatorios/categorias',
          icon: 'pie_chart',
          description: 'Gastos por categoria'
        },
        {
          label: 'Evolução Temporal',
          route: '/relatorios/evolucao',
          icon: 'trending_up',
          description: 'Evolução ao longo do tempo'
        }
      ]
    }
  ];

  constructor(
    private router: Router,
    private authService: AuthService
  ) {}

  ngOnInit() {
    // Track current route
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe((event: NavigationEnd) => {
        this.currentRoute = event.url;
      });

    // Get user info
    this.authService.currentUser.subscribe(user => {
      this.user = user;
    });
  }

  isRouteActive(route: string): boolean {
    return this.currentRoute.startsWith(route);
  }

  getActiveItem(): NavigationItem | null {
    return this.navigationItems.find(item => this.isRouteActive(item.route)) || null;
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  getUserInitials(): string {
    if (!this.user?.name) return 'U';
    return this.user.name
      .split(' ')
      .map((n: string) => n[0])
      .join('')
      .toUpperCase()
      .substring(0, 2);
  }
}
