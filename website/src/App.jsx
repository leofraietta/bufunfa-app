import './App.css'
import { useState } from 'react'
import { ChevronDown, Wallet, Users, BarChart3, Shield, Database, Code, Smartphone, Target, FileText, Import } from 'lucide-react'

// Importando as imagens
import appFinancasImg from './assets/KNCFIcgmTKzK.jpg'
import interfaceImg from './assets/o7rKD5CU1s7e.png'
import arquiteturaImg from './assets/jEsZXOu8b1W2.jpg'
import dashboardImg from './assets/ap5T9yUWI7OI.png'
import planilhaImg from './assets/WwRfemxWFHJx.png'
import cleanArchImg from './assets/pZdAo9yi7AMu.png'

function App() {
  const [activeSection, setActiveSection] = useState('home')

  const scrollToSection = (sectionId) => {
    const element = document.getElementById(sectionId)
    if (element) {
      element.scrollIntoView({ behavior: 'smooth' })
      setActiveSection(sectionId)
    }
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100">
      {/* Header */}
      <header className="bg-white shadow-lg sticky top-0 z-50">
        <nav className="container mx-auto px-6 py-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-2">
              <Wallet className="h-8 w-8 text-blue-600" />
              <h1 className="text-2xl font-bold text-gray-800">Bufunfa</h1>
            </div>
            <div className="hidden md:flex space-x-8">
              <button 
                onClick={() => scrollToSection('home')}
                className="text-gray-600 hover:text-blue-600 transition-colors"
              >
                Início
              </button>
              <button 
                onClick={() => scrollToSection('features')}
                className="text-gray-600 hover:text-blue-600 transition-colors"
              >
                Funcionalidades
              </button>
              <button 
                onClick={() => scrollToSection('architecture')}
                className="text-gray-600 hover:text-blue-600 transition-colors"
              >
                Arquitetura
              </button>
              <button 
                onClick={() => scrollToSection('roadmap')}
                className="text-gray-600 hover:text-blue-600 transition-colors"
              >
                Roadmap
              </button>
            </div>
          </div>
        </nav>
      </header>

      {/* Hero Section */}
      <section id="home" className="py-20">
        <div className="container mx-auto px-6">
          <div className="flex flex-col lg:flex-row items-center">
            <div className="lg:w-1/2 lg:pr-12">
              <h2 className="text-5xl font-bold text-gray-800 mb-6">
                Projeto <span className="text-blue-600">Bufunfa</span>
              </h2>
              <p className="text-xl text-gray-600 mb-8">
                Aplicativo de controle financeiro pessoal que permite aos usuários gerenciar suas finanças de forma intuitiva e detalhada.
              </p>
              <div className="flex flex-col sm:flex-row space-y-4 sm:space-y-0 sm:space-x-4">
                <button 
                  onClick={() => scrollToSection('features')}
                  className="bg-blue-600 text-white px-8 py-3 rounded-lg hover:bg-blue-700 transition-colors"
                >
                  Conhecer Funcionalidades
                </button>
                <button 
                  onClick={() => scrollToSection('architecture')}
                  className="border border-blue-600 text-blue-600 px-8 py-3 rounded-lg hover:bg-blue-50 transition-colors"
                >
                  Ver Arquitetura
                </button>
              </div>
            </div>
            <div className="lg:w-1/2 mt-12 lg:mt-0">
              <img 
                src={appFinancasImg} 
                alt="Aplicativo de Finanças" 
                className="rounded-lg shadow-2xl w-full max-w-md mx-auto"
              />
            </div>
          </div>
        </div>
      </section>

      {/* Visão Geral */}
      <section className="py-16 bg-white">
        <div className="container mx-auto px-6">
          <div className="flex flex-col lg:flex-row items-center">
            <div className="lg:w-1/2 lg:pr-12">
              <h3 className="text-3xl font-bold text-gray-800 mb-6">Visão Geral do Projeto</h3>
              <div className="space-y-4">
                <div className="flex items-center space-x-3">
                  <div className="w-2 h-2 bg-green-500 rounded-full"></div>
                  <span className="text-gray-700">Primeira fase: Aplicação Web</span>
                </div>
                <div className="flex items-center space-x-3">
                  <div className="w-2 h-2 bg-green-500 rounded-full"></div>
                  <span className="text-gray-700">Gestão de finanças pessoais e conjuntas</span>
                </div>
                <div className="flex items-center space-x-3">
                  <div className="w-2 h-2 bg-green-500 rounded-full"></div>
                  <span className="text-gray-700">Interface intuitiva e detalhada</span>
                </div>
                <div className="flex items-center space-x-3">
                  <div className="w-2 h-2 bg-blue-500 rounded-full"></div>
                  <span className="text-gray-700">Planos futuros: Versão móvel</span>
                </div>
              </div>
            </div>
            <div className="lg:w-1/2 mt-8 lg:mt-0">
              <img 
                src={interfaceImg} 
                alt="Interface do Aplicativo" 
                className="rounded-lg shadow-lg w-full"
              />
            </div>
          </div>
        </div>
      </section>

      {/* Funcionalidades */}
      <section id="features" className="py-16 bg-gray-50">
        <div className="container mx-auto px-6">
          <h3 className="text-4xl font-bold text-center text-gray-800 mb-12">Funcionalidades Principais</h3>
          
          <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-8 mb-16">
            <div className="bg-white p-6 rounded-lg shadow-lg">
              <Shield className="h-12 w-12 text-blue-600 mb-4" />
              <h4 className="text-xl font-semibold mb-3">Gestão de Usuários</h4>
              <p className="text-gray-600">Cadastro e login exclusivamente via Google OAuth2 para máxima segurança.</p>
            </div>
            
            <div className="bg-white p-6 rounded-lg shadow-lg">
              <Wallet className="h-12 w-12 text-green-600 mb-4" />
              <h4 className="text-xl font-semibold mb-3">Gestão de Contas</h4>
              <p className="text-gray-600">Contas correntes/principais e cartões de crédito com consolidação automática.</p>
            </div>
            
            <div className="bg-white p-6 rounded-lg shadow-lg">
              <BarChart3 className="h-12 w-12 text-purple-600 mb-4" />
              <h4 className="text-xl font-semibold mb-3">Lançamentos</h4>
              <p className="text-gray-600">Receitas e despesas recorrentes, parceladas e esporádicas com provisionamento.</p>
            </div>
            
            <div className="bg-white p-6 rounded-lg shadow-lg">
              <Users className="h-12 w-12 text-orange-600 mb-4" />
              <h4 className="text-xl font-semibold mb-3">Contas Conjuntas</h4>
              <p className="text-gray-600">Sistema de rateio entre usuários com apuração mensal automática.</p>
            </div>
            
            <div className="bg-white p-6 rounded-lg shadow-lg">
              <BarChart3 className="h-12 w-12 text-indigo-600 mb-4" />
              <h4 className="text-xl font-semibold mb-3">Dashboard</h4>
              <p className="text-gray-600">Visão geral com saldos, projeções e próximos vencimentos.</p>
            </div>
            
            <div className="bg-white p-6 rounded-lg shadow-lg">
              <Target className="h-12 w-12 text-red-600 mb-4" />
              <h4 className="text-xl font-semibold mb-3">Provisionamento</h4>
              <p className="text-gray-600">Sistema inteligente de provisionamento com comparativo real vs. planejado.</p>
            </div>
          </div>

          <div className="flex flex-col lg:flex-row items-center">
            <div className="lg:w-1/2 lg:pr-12">
              <h4 className="text-2xl font-bold text-gray-800 mb-6">Gestão Detalhada de Lançamentos</h4>
              <div className="space-y-4">
                <div className="border-l-4 border-green-500 pl-4">
                  <h5 className="font-semibold text-green-700">Receitas</h5>
                  <p className="text-gray-600">Recorrentes (salário) e esporádicas</p>
                </div>
                <div className="border-l-4 border-red-500 pl-4">
                  <h5 className="font-semibold text-red-700">Despesas Recorrentes</h5>
                  <p className="text-gray-600">Valor provisionado que se repete mensalmente</p>
                </div>
                <div className="border-l-4 border-orange-500 pl-4">
                  <h5 className="font-semibold text-orange-700">Despesas Parceladas</h5>
                  <p className="text-gray-600">Valor fixo por número definido de meses</p>
                </div>
                <div className="border-l-4 border-yellow-500 pl-4">
                  <h5 className="font-semibold text-yellow-700">Provisionamento "Mercado"</h5>
                  <p className="text-gray-600">Comparativo entre gasto real e provisionado</p>
                </div>
              </div>
            </div>
            <div className="lg:w-1/2 mt-8 lg:mt-0">
              <img 
                src={planilhaImg} 
                alt="Planilha de Controle" 
                className="rounded-lg shadow-lg w-full"
              />
            </div>
          </div>
        </div>
      </section>

      {/* Arquitetura */}
      <section id="architecture" className="py-16 bg-white">
        <div className="container mx-auto px-6">
          <h3 className="text-4xl font-bold text-center text-gray-800 mb-12">Arquitetura Técnica</h3>
          
          <div className="flex flex-col lg:flex-row items-center mb-16">
            <div className="lg:w-1/2 lg:pr-12">
              <h4 className="text-2xl font-bold text-gray-800 mb-6">Stack Tecnológico</h4>
              <div className="space-y-4">
                <div className="flex items-center space-x-3">
                  <Database className="h-6 w-6 text-blue-600" />
                  <span className="text-gray-700"><strong>Backend:</strong> .NET 8 (API RESTful)</span>
                </div>
                <div className="flex items-center space-x-3">
                  <Code className="h-6 w-6 text-red-600" />
                  <span className="text-gray-700"><strong>Frontend:</strong> Angular (UI moderna e responsiva)</span>
                </div>
                <div className="flex items-center space-x-3">
                  <Database className="h-6 w-6 text-green-600" />
                  <span className="text-gray-700"><strong>Banco de Dados:</strong> PostgreSQL</span>
                </div>
                <div className="flex items-center space-x-3">
                  <Shield className="h-6 w-6 text-purple-600" />
                  <span className="text-gray-700"><strong>Autenticação:</strong> Google OAuth2</span>
                </div>
                <div className="flex items-center space-x-3">
                  <BarChart3 className="h-6 w-6 text-orange-600" />
                  <span className="text-gray-700"><strong>ORM:</strong> Entity Framework Core</span>
                </div>
                <div className="flex items-center space-x-3">
                  <Target className="h-6 w-6 text-indigo-600" />
                  <span className="text-gray-700"><strong>Padrão:</strong> Clean Architecture</span>
                </div>
              </div>
            </div>
            <div className="lg:w-1/2 mt-8 lg:mt-0">
              <img 
                src={arquiteturaImg} 
                alt="Arquitetura do Sistema" 
                className="rounded-lg shadow-lg w-full"
              />
            </div>
          </div>

          <div className="flex flex-col lg:flex-row-reverse items-center">
            <div className="lg:w-1/2 lg:pl-12">
              <h4 className="text-2xl font-bold text-gray-800 mb-6">Clean Architecture</h4>
              <p className="text-gray-600 mb-4">
                O projeto seguirá os princípios da Clean Architecture para garantir:
              </p>
              <ul className="space-y-2 text-gray-600">
                <li className="flex items-center space-x-2">
                  <div className="w-2 h-2 bg-blue-500 rounded-full"></div>
                  <span>Separação clara de responsabilidades</span>
                </li>
                <li className="flex items-center space-x-2">
                  <div className="w-2 h-2 bg-blue-500 rounded-full"></div>
                  <span>Facilidade de manutenção e testes</span>
                </li>
                <li className="flex items-center space-x-2">
                  <div className="w-2 h-2 bg-blue-500 rounded-full"></div>
                  <span>Escalabilidade e flexibilidade</span>
                </li>
                <li className="flex items-center space-x-2">
                  <div className="w-2 h-2 bg-blue-500 rounded-full"></div>
                  <span>Independência de frameworks</span>
                </li>
              </ul>
            </div>
            <div className="lg:w-1/2 mt-8 lg:mt-0">
              <img 
                src={cleanArchImg} 
                alt="Clean Architecture Diagram" 
                className="rounded-lg shadow-lg w-full"
              />
            </div>
          </div>
        </div>
      </section>

      {/* Dashboard Preview */}
      <section className="py-16 bg-gray-50">
        <div className="container mx-auto px-6">
          <div className="flex flex-col lg:flex-row items-center">
            <div className="lg:w-1/2 lg:pr-12">
              <h3 className="text-3xl font-bold text-gray-800 mb-6">Dashboard Intuitivo</h3>
              <p className="text-gray-600 mb-6">
                A tela inicial apresentará um resumo completo da situação financeira do usuário, 
                incluindo projeções inteligentes baseadas em lançamentos recorrentes e provisionados.
              </p>
              <div className="space-y-3">
                <div className="flex items-center space-x-3">
                  <BarChart3 className="h-5 w-5 text-green-600" />
                  <span className="text-gray-700">Resumo do saldo de todas as contas</span>
                </div>
                <div className="flex items-center space-x-3">
                  <Target className="h-5 w-5 text-blue-600" />
                  <span className="text-gray-700">Projeção do saldo para o final do mês</span>
                </div>
                <div className="flex items-center space-x-3">
                  <BarChart3 className="h-5 w-5 text-purple-600" />
                  <span className="text-gray-700">Projeções para meses futuros</span>
                </div>
              </div>
            </div>
            <div className="lg:w-1/2 mt-8 lg:mt-0">
              <img 
                src={dashboardImg} 
                alt="Dashboard Financeiro" 
                className="rounded-lg shadow-lg w-full"
              />
            </div>
          </div>
        </div>
      </section>

      {/* Roadmap */}
      <section id="roadmap" className="py-16 bg-white">
        <div className="container mx-auto px-6">
          <h3 className="text-4xl font-bold text-center text-gray-800 mb-12">Plano de Implementação</h3>
          
          <div className="grid md:grid-cols-2 gap-8 mb-16">
            <div className="space-y-6">
              <h4 className="text-2xl font-bold text-gray-800">Fases de Desenvolvimento</h4>
              
              <div className="space-y-4">
                <div className="flex items-start space-x-4">
                  <div className="bg-blue-600 text-white rounded-full w-8 h-8 flex items-center justify-center font-bold">1</div>
                  <div>
                    <h5 className="font-semibold text-gray-800">Setup do Projeto</h5>
                    <p className="text-gray-600">Estrutura .NET e workspace Angular</p>
                  </div>
                </div>
                
                <div className="flex items-start space-x-4">
                  <div className="bg-blue-600 text-white rounded-full w-8 h-8 flex items-center justify-center font-bold">2</div>
                  <div>
                    <h5 className="font-semibold text-gray-800">Modelagem do BD</h5>
                    <p className="text-gray-600">Esquema PostgreSQL e migrações EF Core</p>
                  </div>
                </div>
                
                <div className="flex items-start space-x-4">
                  <div className="bg-blue-600 text-white rounded-full w-8 h-8 flex items-center justify-center font-bold">3</div>
                  <div>
                    <h5 className="font-semibold text-gray-800">Backend (API)</h5>
                    <p className="text-gray-600">Autenticação Google, CRUD e lógicas de negócio</p>
                  </div>
                </div>
                
                <div className="flex items-start space-x-4">
                  <div className="bg-blue-600 text-white rounded-full w-8 h-8 flex items-center justify-center font-bold">4</div>
                  <div>
                    <h5 className="font-semibold text-gray-800">Frontend (UI)</h5>
                    <p className="text-gray-600">Componentes Angular e integração com API</p>
                  </div>
                </div>
              </div>
            </div>
            
            <div className="space-y-6">
              <h4 className="text-2xl font-bold text-gray-800">Funcionalidades Futuras</h4>
              
              <div className="space-y-4">
                <div className="flex items-center space-x-3">
                  <BarChart3 className="h-6 w-6 text-green-600" />
                  <span className="text-gray-700">Relatórios e gráficos por categoria</span>
                </div>
                <div className="flex items-center space-x-3">
                  <Target className="h-6 w-6 text-blue-600" />
                  <span className="text-gray-700">Definição de metas financeiras</span>
                </div>
                <div className="flex items-center space-x-3">
                  <Import className="h-6 w-6 text-purple-600" />
                  <span className="text-gray-700">Importação de extratos (OFX/PDF)</span>
                </div>
                <div className="flex items-center space-x-3">
                  <Smartphone className="h-6 w-6 text-orange-600" />
                  <span className="text-gray-700">Expansão para versão móvel</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Footer */}
      <footer className="bg-gray-800 text-white py-12">
        <div className="container mx-auto px-6">
          <div className="flex flex-col md:flex-row justify-between items-center">
            <div className="flex items-center space-x-2 mb-4 md:mb-0">
              <Wallet className="h-8 w-8 text-blue-400" />
              <span className="text-2xl font-bold">Bufunfa</span>
            </div>
            <div className="text-gray-400">
              <p>Projeto de Aplicativo de Controle Financeiro Pessoal</p>
            </div>
          </div>
          <div className="border-t border-gray-700 mt-8 pt-8 text-center text-gray-400">
            <p>&copy; 2024 Projeto Bufunfa. Desenvolvido com React e Tailwind CSS.</p>
          </div>
        </div>
      </footer>
    </div>
  )
}

export default App

