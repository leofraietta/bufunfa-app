import { Component, OnInit } from '@angular/core';
import { RouterOutlet, RouterLink, Router, NavigationEnd } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from './auth/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    RouterOutlet,
    RouterLink,
    CommonModule,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule
  ],
  templateUrl: './app.html',
  styleUrls: ['./app.css']
})
export class AppComponent implements OnInit {
  title = 'Bufunfa - Controle Financeiro Pessoal';

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit() {
    console.log('AppComponent loaded');
    
    // Listen to router events to handle authentication after navigation
    this.router.events.subscribe(event => {
      if (event instanceof NavigationEnd) {
        console.log('Navigation ended to:', event.url);
        
        // Don't redirect if we're on auth callback or already on login
        if (event.url.startsWith('/auth/callback') || event.url === '/login') {
          return;
        }
        
        // If not authenticated and not on login page, redirect to login
        if (!this.authService.isAuthenticated) {
          console.log('Not authenticated, redirecting to login');
          this.router.navigate(['/login']);
        }
      }
    });
  }

  get isAuthenticated(): boolean {
    return this.authService.isAuthenticated;
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}

