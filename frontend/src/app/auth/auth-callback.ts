import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from './auth.service';

@Component({
  selector: 'app-auth-callback',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="auth-callback-container">
      <div class="loading-spinner">
        <h2>Processando autenticação...</h2>
        <p>Aguarde enquanto finalizamos seu login.</p>
      </div>
    </div>
  `,
  styles: [`
    .auth-callback-container {
      display: flex;
      justify-content: center;
      align-items: center;
      height: 100vh;
      background-color: #f5f5f5;
    }
    
    .loading-spinner {
      text-align: center;
      padding: 2rem;
      background: white;
      border-radius: 8px;
      box-shadow: 0 2px 10px rgba(0,0,0,0.1);
    }
    
    h2 {
      color: #333;
      margin-bottom: 1rem;
    }
    
    p {
      color: #666;
    }
  `]
})
export class AuthCallbackComponent implements OnInit {

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private authService: AuthService
  ) {}

  ngOnInit() {
    console.log('AuthCallbackComponent ngOnInit loaded');
    
    // Extrai os parâmetros da URL
    this.route.queryParams.subscribe(params => {
      const token = params['token'];
      const userId = params['userId'];
      const userName = params['userName'];
      const userEmail = params['userEmail'];

      if (token) {
        // Usa o AuthService para armazenar os dados do usuário
        this.authService.setUserData(token, userId, userName, userEmail);

        console.log('Autenticação bem-sucedida!', {
          userId,
          userName,
          userEmail
        });

        // Redireciona para o dashboard após um pequeno delay
        setTimeout(() => {
          this.router.navigate(['/dashboard']);
        }, 2000);
      } else {
        // Se não há token, redireciona para login
        console.error('Token não encontrado na URL');
        this.router.navigate(['/login']);
      }
    });
  }
}
