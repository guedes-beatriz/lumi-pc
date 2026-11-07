# Gotinha - Lembrete de Água

<div align="center">

<img src="mascote.png" width="165" alt="Mascote Gotinha">

<br>

**Um aplicativo desktop nativo para Windows (`.exe`), moderno, leve, acolhedor e com mascote animada para cuidar da sua hidratação e bem-estar.**

<br>

[![Windows](https://img.shields.io/badge/Plataforma-Windows%2010%20%7C%2011-0078D6?style=for-the-badge&logo=windows&logoColor=white)](https://github.com/guedes-beatriz/app-pc)
[![C#](https://img.shields.io/badge/C%23-WPF%20Desktop-239120?style=for-the-badge&logo=c-sharp&logoColor=white)](https://github.com/guedes-beatriz/app-pc)
[![.NET](https://img.shields.io/badge/.NET-4.8%20Framework-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://github.com/guedes-beatriz/app-pc)
[![Offline](https://img.shields.io/badge/Privacidade-100%25%20Offline-059669?style=for-the-badge)](https://github.com/guedes-beatriz/app-pc)
[![Modo Escuro](https://img.shields.io/badge/Tema-Claro%20%26%20Escuro-0284C7?style=for-the-badge)](https://github.com/guedes-beatriz/app-pc)

</div>

---

## Como Baixar e Testar (Para Amigos)

Não é necessário instalar programas pesados nem ter conhecimentos técnicos para usar a Gotinha.

### Opção 1: Baixar o Pacote Portátil Pronto (Recomendado)
1. Baixe o arquivo zip direto aqui pelo GitHub: **[`dist/Gotinha-v2.0-Portable.zip`](dist/Gotinha-v2.0-Portable.zip)** (ou clique no botão verde **Code** ➔ **Download ZIP** no topo do repositório).
2. Extraia o arquivo zip em qualquer pasta do seu computador (como sua Área de Trabalho ou Documentos).
3. Dê **dois cliques em `Gotinha.exe`** e pronto! O app abrirá com uma animação de boas-vindas.
4. **Dica:** Clique com o botão direito em `Gotinha.exe` ➔ *Enviar para* ➔ *Área de trabalho (criar atalho)* para abrir com 1 clique sempre que ligar o PC.

### Opção 2: Compilar do Código-Fonte
Se você clonou o repositório e deseja compilar o executável:
- Dê um duplo clique em **`compilar.bat`**.
- O Windows usará o compilador nativo C# (`csc.exe`) e gerará o `Gotinha.exe` em menos de 2 segundos.

---

## Principais Recursos

### Mascote Interativa e Expressiva
- **Animações fluidas:** A Gotinha pisca, flutua suavemente e pula a cada gole registrado.
- **Frases motivacionais:** Clique na Gotinha para receber lembretes carinhosos e motivacionais.
- **Celebração:** Ao atingir 100% da sua meta diária, a Gotinha ganha uma coroa dourada e comemora com você.

### Cards de Bebidas
- Cores pastéis e identificação rápida:
  - **Gole:** +150 ml
  - **Copo:** +250 ml *(com destaque e brilho suave)*
  - **Grande:** +350 ml
  - **Garrafa:** +500 ml
  - **Personalizado:** digite qualquer quantidade em ml.
- **Clique tátil com efeito de mola:** Transição suave e responsiva ao registrar água.
- **Pílula de Desfazer:** Registrou por engano? Desfaça com um clique ou com o atalho `Ctrl + Z`.

### Temporizador Zen
- Cápsula fluida com gradiente que se preenche com o passar do tempo.
- Pílulas táteis para ajustar o intervalo de lembrete com facilidade: **`15m`**, **`30m`**, **`45m`**, **`60m`**.
- Botão de pausa para momentos de descanso ou reuniões.

### Histórico e Gráfico Semanal
- **Abas em pílula segmentada:** Alterne com suavidade entre **Registros de Hoje** e **Gráfico 7 Dias**.
- **Cards de resumo:** Veja seu consumo total de hoje, média diária e quantidade de dias com meta batida na semana.
- **Gráfico de barras:** Barras que crescem com animação suave, com indicador de estrela nos dias em que a meta foi atingida.

### Modo Escuro (Oceano Noturno)
- Alterne entre o **Modo Claro (Brisa Aquática)** e o **Modo Escuro (Oceano Noturno)** com 1 clique no ícone da barra de ferramentas ou pressionando `Ctrl + D`.
- Paleta noturna com azul marinho profundo (`#0F172A`) e ciano (`#38BDF8`), calibrada para descanso visual.

### Lembretes e Bandeja do Sistema
- Lembretes visuais na tela com botões em formato de pílula.
- Notificações nativas integradas à Central de Ações do Windows.
- Efeitos sonoros suaves de gota e sino ao registrar água.
- Minimização para a bandeja (perto do relógio do Windows) para rodar em segundo plano.

### 100% Offline e Privado
- Não requer internet, cadastro ou login.
- Todos os registros ficam salvos localmente no seu computador no arquivo `dados_agua.json`.

---

## Atalhos Rápidos de Teclado

| Tecla | Ação |
|:---:|---|
| <kbd>Espaço</kbd> | Registra um Copo de água (+250 ml) |
| <kbd>Ctrl</kbd> + <kbd>Z</kbd> | Desfaz o último registro de água |
| <kbd>Ctrl</kbd> + <kbd>D</kbd> | Alterna entre Modo Claro e Modo Escuro |
| <kbd>Ctrl</kbd> + <kbd>H</kbd> | Alterna entre a tela da Gotinha e o Histórico |
| <kbd>Ctrl</kbd> + <kbd>P</kbd> | Pausa ou retoma o temporizador de lembretes |
| <kbd>Esc</kbd> | Minimiza o aplicativo para a bandeja do sistema |

---

## Estrutura do Projeto

A pasta do projeto é organizada de maneira limpa e modular:

```text
Água/
│
├── Gotinha.exe             # Aplicativo compilado pronto para rodar (Windows)
├── GotinhaApp.cs           # Código-fonte C# da aplicação principal (WPF)
├── SplashWindow.cs         # Código-fonte da tela de abertura animada
├── compilar.bat            # Script em lote para compilar o app em 1 clique
├── mascote.png             # Mascote Gotinha oficial em alta resolução
├── app.ico                 # Ícone oficial do aplicativo (.ico)
├── dados_agua.json         # Banco de dados local em formato JSON
├── README.md               # Documentação do projeto
├── .gitignore              # Regras de arquivos ignorados pelo Git
│
├── dist/                   # Pacotes para distribuição
│   ├── Gotinha-v2.0-Portable.zip  # Pacote portátil pronto para envio
│   └── LEIA-ME.txt                # Instruções rápidas de uso
│
├── web/                    # Versão Web interativa do Gotinha
│   ├── index.html
│   ├── style.css
│   └── app.js
│
└── tools/                  # Ferramentas e scripts auxiliares
    └── image_tools/        # Scripts de tratamento e upscaling de imagens
```

---

## Tecnologias Utilizadas

- **Linguagem:** C# (.NET Framework 4.8 / compatível com C# 5)
- **Framework de UI:** Windows Presentation Foundation (WPF)
- **Renderização e Gráficos:** XAML, Geometry Paths, DropShadowEffects, RenderTransforms
- **Áudio:** Geração algorítmica de ondas sonoras senoidais via buffer PCM nativo (`System.IO.MemoryStream`)
- **Persistência:** `System.Web.Extensions` (`JavaScriptSerializer`)

---

## Créditos

Desenvolvido por **Beatriz Guedes** para tornar o hábito de beber água mais leve, agradável e saudável.
