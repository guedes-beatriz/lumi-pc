# Lumi - Lembrete de Água

<div align="center">

<img src="mascote.png" width="165" alt="Mascote Lumi">

<br>

**Um aplicativo desktop nativo para Windows (`.exe`), moderno, leve, acolhedor e com mascote animada para cuidar da sua hidratação e bem-estar — extremamente econômico em memória RAM e CPU.**

<br>

[![Windows](https://img.shields.io/badge/Plataforma-Windows%2010%20%7C%2011-0078D6?style=for-the-badge&logo=windows&logoColor=white)](https://github.com/guedes-beatriz/lumi-pc)
[![C#](https://img.shields.io/badge/C%23-WPF%20Desktop-239120?style=for-the-badge&logo=c-sharp&logoColor=white)](https://github.com/guedes-beatriz/lumi-pc)
[![.NET](https://img.shields.io/badge/.NET-4.8%20Framework-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://github.com/guedes-beatriz/lumi-pc)
[![Recursos](https://img.shields.io/badge/Desempenho-Baixo%20Uso%20RAM%20%26%20CPU-0284C7?style=for-the-badge)](https://github.com/guedes-beatriz/lumi-pc)
[![Offline](https://img.shields.io/badge/Privacidade-100%25%20Offline-059669?style=for-the-badge)](https://github.com/guedes-beatriz/lumi-pc)
[![Modo Escuro](https://img.shields.io/badge/Tema-Claro%20%26%20Escuro-0284C7?style=for-the-badge)](https://github.com/guedes-beatriz/lumi-pc)

</div>

---

## Como Baixar e Usar

A Lumi foi pensada para ser simples e acessível para qualquer pessoa usar no dia a dia, sem necessidade de instaladores pesados ou cadastros.

### Opção 1: Versão Portátil Pronta (Recomendado)
1. Baixe o pacote pronto diretamente aqui pelo GitHub: **[`dist/Lumi-v2.0-Portable.zip`](dist/Lumi-v2.0-Portable.zip)** (ou clique no botão verde **Code** ➔ **Download ZIP** no topo da página).
2. Extraia o arquivo zip em qualquer pasta do seu computador (como sua Área de Trabalho ou Documentos).
3. Dê **dois cliques em `Lumi.exe`** para iniciar o aplicativo.
4. **Dica:** Clique com o botão direito em `Lumi.exe` ➔ *Enviar para* ➔ *Área de trabalho (criar atalho)* para ter acesso rápido sempre que ligar o computador.

### Opção 2: Compilar a partir do Código-Fonte
Se preferir compilar diretamente do código-fonte:
- Execute o script **`compilar.bat`** com um duplo clique.
- O compilador nativo C# do Windows (`csc.exe`) gerará o executável `Lumi.exe` em poucos segundos, sem precisar de ferramentas adicionais.

---

## Funcionalidades e Recursos

A Lumi combina acompanhamento de saúde com uma interface gentil e personalizável, pensada para quem passa muito tempo em frente ao computador e quer manter o hábito de beber água de forma natural.

### Mascote Interativa
A Lumi é uma companheira de bem-estar que reage à sua evolução ao longo do dia:
- Animações fluidas de respiração, flutuação e pulos a cada registro de água.
- Mensagens de incentivo carinhosas ao interagir com ela.
- Celebração visual quando você alcança 100% da sua meta diária.

### Modo Widget Flutuante (Canto da Tela)
Acompanhe sua hidratação sem interromper suas atividades:
- Design horizontal compacto e moderno, ideal para se posicionar no canto da tela.
- Opção para fixar sempre visível sobre outras janelas com alinhamento magnético nas bordas da tela.
- Campo direto para digitar quantos ml consumiu e registrar com a tecla Enter, sem precisar abrir a janela principal.

### Registro Fácil e Adaptável de Água
Adapte o app à sua rotina e ao recipiente que você costuma usar:
- Botões rápidos com volumes pré-definidos: **Gole (75 ml)**, **Copo (200 ml)**, **Grande (350 ml)** e **Garrafa (500 ml)**.
- Campo para inserção de qualquer volume personalizado em ml.
- Botão e atalho de desfazer caso registre algum valor por engano.

### Lembretes Inteligentes e Temporizador
Nunca mais esqueça de beber água durante o trabalho ou estudo:
- Intervalos programáveis de **15**, **30**, **45** ou **60 minutos**.
- Notificações nativas na Central de Ações do Windows.
- Efeitos sonoros suaves para avisos discretos e agradáveis.
- Modo pausa para momentos de reuniões, descanso ou foco profundo.

### Histórico e Visualização de Metas
Acompanhe sua consistência de forma clara:
- Linha do tempo com todos os registros e horários do dia.
- Gráfico semanal com o consumo dos últimos 7 dias e indicação das metas alcançadas.
- Cálculo de meta diária recomendada com base no peso corporal.

### Leveza e Baixo Consumo de Memória e CPU
Construído de forma nativa e ultra otimizada para acompanhar sua rotina sem pesar:
- **Uso mínimo de memória RAM e CPU:** O aplicativo é extremamente leve e econômico, consumindo uma fração mínima de memória e praticamente 0% de CPU em repouso.
- **Perfeito para segundo plano:** Pode ficar ativo o dia inteiro na bandeja ou em widget sem comprometer o desempenho de jogos, navegadores com dezenas de abas abertas ou programas de trabalho pesados.

### Modos Claro e Escuro
Conforto visual a qualquer momento do dia:
- **Modo Claro (Brisa Aquática):** Paleta suave com tons de azul e branco.
- **Modo Escuro (Oceano Noturno):** Tons profundos de azul marinho calibrados para evitar cansaço visual em ambientes com pouca luz.

### Privacidade e Funcionamento Offline
- Não requer conexão com a internet, login ou permissões invasivas.
- Todos os registros ficam salvos localmente e com total privacidade no arquivo `dados_agua.json`.

---

## Atalhos Rápidos de Teclado

| Tecla | Ação |
|:---:|---|
| <kbd>Espaço</kbd> | Registra um Copo de água (+200 ml) |
| <kbd>Ctrl</kbd> + <kbd>Z</kbd> | Desfaz o último registro de água |
| <kbd>Ctrl</kbd> + <kbd>D</kbd> | Alterna entre Modo Claro e Modo Escuro |
| <kbd>Ctrl</kbd> + <kbd>H</kbd> | Alterna entre a tela da Lumi e o Histórico |
| <kbd>Ctrl</kbd> + <kbd>P</kbd> | Pausa ou retoma o temporizador de lembretes |
| <kbd>Esc</kbd> | Minimiza o aplicativo para a bandeja do sistema |

---

## Estrutura do Projeto

A pasta do projeto é organizada de maneira limpa e modular:

```text
Água/
│
├── Lumi.exe                # Aplicativo compilado pronto para rodar (Windows)
├── LumiApp.cs              # Código-fonte C# da aplicação principal e Widget (WPF)
├── SplashWindow.cs         # Código-fonte da tela de abertura animada
├── compilar.bat            # Script em lote para compilar o app em 1 clique
├── mascote.png             # Mascote Lumi oficial em alta resolução
├── icone.ico               # Ícone oficial do aplicativo (.ico)
├── dados_agua.json         # Banco de dados local em formato JSON
├── README.md               # Documentação do projeto
├── .gitignore              # Regras de arquivos ignorados pelo Git
│
├── dist/                   # Pacotes para distribuição
│   ├── Lumi-v2.0-Portable.zip     # Pacote portátil pronto para uso
│   └── LEIA-ME.txt                # Instruções rápidas de uso
│
├── web/                    # Versão Web interativa da Lumi
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

## Desenvolvedora

Desenvolvido por **Beatriz Guedes** para tornar o hábito de beber água mais leve, agradável e saudável.
