import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { RouterOutlet, Router, NavigationEnd } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from './auth/auth.service';
import { NavigationComponent } from './shared/navigation/navigation';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    RouterOutlet,
    CommonModule,
    NavigationComponent
  ],
  templateUrl: './app.html',
  styleUrls: ['./app.css']
})
export class AppComponent implements OnInit {
  title = 'Bufunfa - Controle Financeiro Pessoal';
  isAuthenticated = false;

  constructor(
    private authService: AuthService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    console.log('AppComponent loaded');
    
    // Subscribe to authentication changes
    this.authService.currentUser.subscribe(user => {
      this.isAuthenticated = !!user && !!this.authService.getToken();
      this.cdr.detectChanges();
    });
    
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

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}

