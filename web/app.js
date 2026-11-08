/**
 * Lembrete de Água - Gotinha
 * Um aplicativo bonitinho, intuitivo e completo para hidratação no PC.
 */

(function () {
  'use strict';

  // --- Sons via Web Audio API (100% offline, sem arquivos externos) ---
  class SoundManager {
    constructor() {
      this.ctx = null;
      this.enabled = true;
    }

    init() {
      if (!this.ctx) {
        const AudioCtx = window.AudioContext || window.webkitAudioContext;
        if (AudioCtx) {
          this.ctx = new AudioCtx();
        }
      }
      if (this.ctx && this.ctx.state === 'suspended') {
        this.ctx.resume();
      }
    }

    playWaterDrop() {
      if (!this.enabled) return;
      this.init();
      if (!this.ctx) return;

      const now = this.ctx.currentTime;

      // Oscilador 1: Som de gota de água modulando frequência
      const osc = this.ctx.createOscillator();
      const gain = this.ctx.createGain();

      osc.type = 'sine';
      osc.frequency.setValueAtTime(600, now);
      osc.frequency.exponentialRampToValueAtTime(1400, now + 0.08);
      osc.frequency.exponentialRampToValueAtTime(700, now + 0.18);

      gain.gain.setValueAtTime(0.001, now);
      gain.gain.linearRampToValueAtTime(0.28, now + 0.02);
      gain.gain.exponentialRampToValueAtTime(0.0001, now + 0.22);

      osc.connect(gain);
      gain.connect(this.ctx.destination);

      osc.start(now);
      osc.stop(now + 0.25);

      // Segunda gota suave (eco gostoso)
      const osc2 = this.ctx.createOscillator();
      const gain2 = this.ctx.createGain();

      osc2.type = 'sine';
      osc2.frequency.setValueAtTime(900, now + 0.1);
      osc2.frequency.exponentialRampToValueAtTime(1800, now + 0.16);

      gain2.gain.setValueAtTime(0.001, now + 0.1);
      gain2.gain.linearRampToValueAtTime(0.15, now + 0.12);
      gain2.gain.exponentialRampToValueAtTime(0.0001, now + 0.28);

      osc2.connect(gain2);
      gain2.connect(this.ctx.destination);

      osc2.start(now + 0.1);
      osc2.stop(now + 0.3);
    }

    playReminderChime() {
      if (!this.enabled) return;
      this.init();
      if (!this.ctx) return;

      const notes = [523.25, 659.25, 783.99, 1046.50]; // C5, E5, G5, C6 (Acorde agradável)
      const now = this.ctx.currentTime;

      notes.forEach((freq, i) => {
        const osc = this.ctx.createOscillator();
        const gain = this.ctx.createGain();

        osc.type = 'triangle';
        osc.frequency.setValueAtTime(freq, now + i * 0.12);

        gain.gain.setValueAtTime(0.001, now + i * 0.12);
        gain.gain.linearRampToValueAtTime(0.22, now + i * 0.12 + 0.03);
        gain.gain.exponentialRampToValueAtTime(0.0001, now + i * 0.12 + 0.6);

        osc.connect(gain);
        gain.connect(this.ctx.destination);

        osc.start(now + i * 0.12);
        osc.stop(now + i * 0.12 + 0.65);
      });
    }

    playCelebrationFanfare() {
      if (!this.enabled) return;
      this.init();
      if (!this.ctx) return;

      const notes = [
        { f: 523.25, d: 0.12 }, // C5
        { f: 659.25, d: 0.12 }, // E5
        { f: 783.99, d: 0.12 }, // G5
        { f: 1046.50, d: 0.4 }, // C6
      ];

      let startTime = this.ctx.currentTime;
      notes.forEach((item) => {
        const osc = this.ctx.createOscillator();
        const gain = this.ctx.createGain();

        osc.type = 'sine';
        osc.frequency.setValueAtTime(item.f, startTime);

        gain.gain.setValueAtTime(0.001, startTime);
        gain.gain.linearRampToValueAtTime(0.25, startTime + 0.02);
        gain.gain.exponentialRampToValueAtTime(0.0001, startTime + item.d);

        osc.connect(gain);
        gain.connect(this.ctx.destination);

        osc.start(startTime);
        osc.stop(startTime + item.d + 0.05);

        startTime += item.d * 0.8;
      });
    }
  }

  // --- Confete de Celebração ---
  class Confetti {
    constructor(canvas) {
      this.canvas = canvas;
      this.ctx = canvas.getContext('2d');
      this.particles = [];
      this.animId = null;
      this.resize();
      window.addEventListener('resize', () => this.resize());
    }

    resize() {
      this.canvas.width = window.innerWidth;
      this.canvas.height = window.innerHeight;
    }

    fire() {
      this.particles = [];
      const colors = ['#38bdf8', '#0284c7', '#ff769c', '#facc15', '#a78bfa', '#34d399'];
      for (let i = 0; i < 90; i++) {
        this.particles.push({
          x: this.canvas.width / 2 + (Math.random() - 0.5) * 80,
          y: this.canvas.height / 2,
          vx: (Math.random() - 0.5) * 14,
          vy: (Math.random() - 0.8) * 16,
          size: Math.random() * 8 + 4,
          color: colors[Math.floor(Math.random() * colors.length)],
          rotation: Math.random() * 360,
          vRot: (Math.random() - 0.5) * 10,
          alpha: 1,
        });
      }

      if (this.animId) cancelAnimationFrame(this.animId);
      this.animate();
    }

    animate() {
      this.ctx.clearRect(0, 0, this.canvas.width, this.canvas.height);
      let aliveCount = 0;

      for (let p of this.particles) {
        p.x += p.vx;
        p.y += p.vy;
        p.vy += 0.35; // gravidade
        p.rotation += p.vRot;
        p.alpha -= 0.007;

        if (p.alpha > 0) {
          aliveCount++;
          this.ctx.save();
          this.ctx.globalAlpha = Math.max(0, p.alpha);
          this.ctx.translate(p.x, p.y);
          this.ctx.rotate((p.rotation * Math.PI) / 180);
          this.ctx.fillStyle = p.color;
          this.ctx.fillRect(-p.size / 2, -p.size / 2, p.size, p.size * 1.4);
          this.ctx.restore();
        }
      }

      if (aliveCount > 0) {
        this.animId = requestAnimationFrame(() => this.animate());
      } else {
        this.ctx.clearRect(0, 0, this.canvas.width, this.canvas.height);
      }
    }
  }

  // --- Dicas de Hidratação ---
  const HEALTH_TIPS = [
    'Beber água melhora a concentração, alivia dores de cabeça e dá mais energia no PC! 💡',
    'Dica de ouro: tenha sempre um copo ou garrafinha ao lado do teclado! 💧',
    'Sabia? Muitas vezes quando sentimos sono à tarde no PC, é apenas desidratação leve! 😴',
    'Seus olhos e pele agradecem cada gole de água durante longas sessões no computador! ✨',
    'Beber água antes de sentir sede é o segredo para se manter focada e produtiva! 🚀',
    'Uma pausa para um gole d\'água também é uma ótima desculpa para esticar as costas! 🧘‍♀️',
  ];

  // --- Aplicação Principal ---
  class WaterApp {
    constructor() {
      this.sound = new SoundManager();
      this.confetti = new Confetti(document.getElementById('confettiCanvas'));

      this.config = this.loadConfig();
      this.todayData = this.loadTodayData();

      this.timerSeconds = this.config.interval * 60;
      this.timerInterval = null;
      this.isTimerPaused = false;
      this.originalTitle = document.title;
      this.flashTitleInterval = null;

      this.initDom();
      this.initEvents();
      this.startTimer();
      this.render();
      this.startMascotBlinking();
      this.rotateTip();
    }

    getTodayKey() {
      const d = new Date();
      return `water_log_${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
    }

    loadConfig() {
      const saved = localStorage.getItem('water_app_config');
      const defaults = {
        target: 2000,
        interval: 30, // minutos
        soundEnabled: true,
        notifications: false,
      };
      return saved ? { ...defaults, ...JSON.parse(saved) } : defaults;
    }

    saveConfig() {
      localStorage.setItem('water_app_config', JSON.stringify(this.config));
    }

    loadTodayData() {
      const key = this.getTodayKey();
      const saved = localStorage.getItem(key);
      if (saved) {
        return JSON.parse(saved);
      }
      return { intake: 0, drinks: [] };
    }

    saveTodayData() {
      const key = this.getTodayKey();
      localStorage.setItem(key, JSON.stringify(this.todayData));
    }

    initDom() {
      // Elementos principais
      this.dom = {
        currentIntake: document.getElementById('currentIntake'),
        targetIntake: document.getElementById('targetIntake'),
        percentageText: document.getElementById('percentageText'),
        progressBarFill: document.getElementById('progressBarFill'),
        waterFill: document.getElementById('waterFill'),
        quickStatus: document.getElementById('quickStatus'),

        mascotEyes: document.getElementById('mascotEyes'),
        mascotMouth: document.getElementById('mascotMouth'),
        mascotCrown: document.getElementById('mascotCrown'),
        mascotMessage: document.getElementById('mascotMessage'),
        mascotContainer: document.getElementById('mascotContainer'),

        timerDisplay: document.getElementById('timerDisplay'),
        btnTimerToggle: document.getElementById('btnTimerToggle'),
        timerToggleIcon: document.getElementById('timerToggleIcon'),
        btnTestReminder: document.getElementById('btnTestReminder'),
        intervalChips: document.getElementById('intervalChips'),

        btnUndo: document.getElementById('btnUndo'),
        customAmountForm: document.getElementById('customAmountForm'),
        customAmountInput: document.getElementById('customAmountInput'),

        btnSoundToggle: document.getElementById('btnSoundToggle'),
        soundIcon: document.getElementById('soundIcon'),
        btnSettingsToggle: document.getElementById('btnSettingsToggle'),
        settingsModal: document.getElementById('settingsModal'),
        btnCloseSettings: document.getElementById('btnCloseSettings'),
        btnSaveSettings: document.getElementById('btnSaveSettings'),
        inputTarget: document.getElementById('inputTarget'),
        inputWeight: document.getElementById('inputWeight'),
        btnCalcTarget: document.getElementById('btnCalcTarget'),
        btnToggleNotifications: document.getElementById('btnToggleNotifications'),
        notificationStatusText: document.getElementById('notificationStatusText'),
        btnResetToday: document.getElementById('btnResetToday'),

        reminderAlertModal: document.getElementById('reminderAlertModal'),
        btnAlertDrink250: document.getElementById('btnAlertDrink250'),
        btnAlertDrink350: document.getElementById('btnAlertDrink350'),
        btnAlertSnooze: document.getElementById('btnAlertSnooze'),

        historyList: document.getElementById('historyList'),
        drinksCount: document.getElementById('drinksCount'),
        weekChart: document.getElementById('weekChart'),
        weekSummary: document.getElementById('weekSummary'),
        tipText: document.getElementById('tipText'),
        toastFeedback: document.getElementById('toastFeedback'),
      };

      this.toastTimeout = null;

      // Configuração de som
      this.sound.enabled = this.config.soundEnabled;
      this.updateSoundIcon();

      // Preencher campos de configurações
      this.dom.inputTarget.value = this.config.target;
      this.updateIntervalChips();
      this.checkNotificationStatus();
    }

    initEvents() {
      // Cliques nos botões de bebida rápida com feedback visual tátil
      document.querySelectorAll('.drink-btn').forEach((btn) => {
        btn.addEventListener('click', () => {
          btn.classList.add('clicked');
          setTimeout(() => btn.classList.remove('clicked'), 350);
          const amount = parseInt(btn.dataset.amount, 10);
          const icon = btn.querySelector('.btn-emoji').textContent;
          const name = btn.querySelector('.btn-name').textContent;
          this.addWater(amount, `${name} (${icon})`);
        });
      });

      // Quantidade personalizada
      this.dom.customAmountForm.addEventListener('submit', (e) => {
        e.preventDefault();
        const val = parseInt(this.dom.customAmountInput.value, 10);
        if (val && val > 0) {
          this.addWater(val, 'Personalizado 💧');
          this.dom.customAmountInput.value = '';
        }
      });

      // Desfazer
      this.dom.btnUndo.addEventListener('click', () => this.undoLastDrink());

      // Chips de intervalo de tempo
      this.dom.intervalChips.addEventListener('click', (e) => {
        const chip = e.target.closest('.chip');
        if (!chip) return;
        const minutes = parseInt(chip.dataset.min, 10);
        this.setIntervalMinutes(minutes);
      });

      // Pausar / Retomar Timer
      this.dom.btnTimerToggle.addEventListener('click', () => this.toggleTimer());

      // Testar Lembrete
      this.dom.btnTestReminder.addEventListener('click', () => this.triggerReminderAlert());

      // Som
      this.dom.btnSoundToggle.addEventListener('click', () => this.toggleSound());

      // Modais
      this.dom.btnSettingsToggle.addEventListener('click', () => this.openSettings());
      this.dom.btnCloseSettings.addEventListener('click', () => this.closeSettings());
      this.dom.btnSaveSettings.addEventListener('click', () => this.saveSettings());
      this.dom.btnResetToday.addEventListener('click', () => this.resetToday());

      // Fechar modais ao clicar no backdrop (fora do card)
      [this.dom.settingsModal, this.dom.reminderAlertModal].forEach((modal) => {
        if (!modal) return;
        modal.addEventListener('click', (e) => {
          if (e.target === modal) {
            if (modal === this.dom.settingsModal) this.closeSettings();
            if (modal === this.dom.reminderAlertModal) this.closeReminderAlert();
          }
        });
      });

      // Fechar modais com tecla ESC
      window.addEventListener('keydown', (e) => {
        if (e.key === 'Escape') {
          this.closeSettings();
          this.closeReminderAlert();
        }
      });

      // Cálculo por peso
      this.dom.btnCalcTarget.addEventListener('click', () => {
        const weight = parseFloat(this.dom.inputWeight.value);
        if (weight && weight > 25) {
          const recommended = Math.round(weight * 35);
          this.dom.inputTarget.value = recommended;
        } else {
          alert('Por favor, informe um peso válido em kg.');
        }
      });

      // Ativar Notificações do Windows
      this.dom.btnToggleNotifications.addEventListener('click', () => this.requestNotifications());

      // Ações do Modal de Alerta
      this.dom.btnAlertDrink250.addEventListener('click', () => {
        this.addWater(200, 'Copo (🥛)');
        this.closeReminderAlert();
      });

      this.dom.btnAlertDrink350.addEventListener('click', () => {
        this.addWater(350, 'Copo Grande (🥤)');
        this.closeReminderAlert();
      });

      this.dom.btnAlertSnooze.addEventListener('click', () => {
        this.snoozeTimer(5);
        this.closeReminderAlert();
      });

      // Cliques no mascote (interação fofa)
      this.dom.mascotContainer.addEventListener('click', () => {
        this.pokeMascot();
      });

      // Abas de Histórico
      document.querySelectorAll('.tab-btn').forEach((tab) => {
        tab.addEventListener('click', () => {
          document.querySelectorAll('.tab-btn').forEach((t) => t.classList.remove('active'));
          document.querySelectorAll('.tab-content').forEach((c) => c.classList.remove('active'));
          tab.classList.add('active');
          if (tab.dataset.tab === 'today') {
            document.getElementById('tabToday').classList.add('active');
          } else {
            document.getElementById('tabWeek').classList.add('active');
            this.renderWeekChart();
          }
        });
      });

      // Limpar título piscante quando a janela ganhar foco
      window.addEventListener('focus', () => {
        this.stopFlashingTitle();
      });
    }

    showToast(msg, icon = '💧') {
      const toast = this.dom.toastFeedback;
      if (!toast) return;
      toast.innerHTML = `<span class="toast-icon">${icon}</span> <span class="toast-msg">${msg}</span>`;
      toast.classList.remove('hidden');
      void toast.offsetWidth; // Forçar reflow para reiniciar transição
      toast.classList.add('show');

      if (this.toastTimeout) clearTimeout(this.toastTimeout);
      this.toastTimeout = setTimeout(() => {
        toast.classList.remove('show');
        setTimeout(() => {
          if (!toast.classList.contains('show')) {
            toast.classList.add('hidden');
          }
        }, 280);
      }, 2200);
    }

    triggerDrinkFeedback(amount) {
      // Pulso suave no copo de água
      const glass = document.querySelector('.water-glass');
      if (glass) {
        glass.classList.remove('fill-pulse');
        void glass.offsetWidth;
        glass.classList.add('fill-pulse');
      }

      // Pop animado no número de ml consumido
      if (this.dom.currentIntake) {
        this.dom.currentIntake.classList.remove('stat-pop');
        void this.dom.currentIntake.offsetWidth;
        this.dom.currentIntake.classList.add('stat-pop');
      }

      // Mascote Gotinha celebra com pulinho alegre
      if (this.dom.mascotContainer) {
        this.dom.mascotContainer.classList.remove('happy-bounce');
        void this.dom.mascotContainer.offsetWidth;
        this.dom.mascotContainer.classList.add('happy-bounce');
      }
    }

    addWater(amount, label = 'Água') {
      const now = new Date();
      const timeStr = now.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });

      const wasBelow100 = this.todayData.intake < this.config.target;

      this.todayData.intake += amount;
      this.todayData.drinks.unshift({
        id: Date.now(),
        amount: amount,
        label: label,
        time: timeStr,
      });

      this.saveTodayData();
      this.render();

      // Feedback visual fluido e responsivo
      this.triggerDrinkFeedback(amount);

      // Som, comemoração e toast
      const isNow100 = this.todayData.intake >= this.config.target;
      if (wasBelow100 && isNow100) {
        this.sound.playCelebrationFanfare();
        this.confetti.fire();
        this.showToast('🎉 Parabéns! Meta de água atingida!', '🌟');
      } else {
        this.sound.playWaterDrop();
        this.showToast(`+${amount} ml registrado! Muito bem!`, '💧');
      }

      // Reinicia o timer de lembrete já que acabou de beber
      this.resetTimer();
    }

    undoLastDrink() {
      if (this.todayData.drinks.length === 0) return;
      const last = this.todayData.drinks.shift();
      this.todayData.intake = Math.max(0, this.todayData.intake - last.amount);
      this.saveTodayData();
      this.render();
      this.sound.playWaterDrop();
      this.showToast(`Gole de -${last.amount} ml desfeito`, '↩');
    }

    render() {
      const intake = this.todayData.intake;
      const target = this.config.target;
      const pct = Math.min(100, Math.round((intake / target) * 100));

      this.dom.currentIntake.textContent = intake;
      this.dom.targetIntake.textContent = target;
      this.dom.percentageText.textContent = `${pct}%`;
      this.dom.progressBarFill.style.width = `${pct}%`;
      this.dom.waterFill.style.height = `${pct}%`;

      // Status text
      const remaining = target - intake;
      if (remaining > 0) {
        this.dom.quickStatus.innerHTML = `Faltam <strong>${remaining} ml</strong> para a sua meta! ✨`;
      } else {
        const extra = intake - target;
        this.dom.quickStatus.innerHTML = `🎉 Meta batida! ${extra > 0 ? `(+${extra} ml extras)` : ''} Hidratação perfeita!`;
      }

      // Mascote Gotinha
      this.updateMascot(pct);

      // Histórico de Hoje
      this.renderTodayHistory();
    }

    setMascotMessage(text) {
      const msg = this.dom.mascotMessage;
      if (!msg || msg.textContent === text) return;
      msg.classList.add('bubble-transitioning');
      setTimeout(() => {
        msg.textContent = text;
        msg.classList.remove('bubble-transitioning');
      }, 140);
    }

    updateMascot(pct) {
      const crown = this.dom.mascotCrown;
      const mouth = this.dom.mascotMouth;

      if (pct === 0) {
        crown.classList.add('hidden');
        mouth.setAttribute('d', 'M 92 172 Q 100 166 108 172'); // leve boca neutra
        this.setMascotMessage('Oi! Que tal começar o dia com um copão d\'água fresquinha? 💧');
      } else if (pct < 30) {
        crown.classList.add('hidden');
        mouth.setAttribute('d', 'M 90 170 Q 100 178 110 170'); // sorriso tímido
        this.setMascotMessage('Muito bem! Primeiro passo dado, seu corpinho agradece! 💧');
      } else if (pct < 70) {
        crown.classList.add('hidden');
        mouth.setAttribute('d', 'M 88 168 Q 100 183 112 168'); // sorriso aberto
        this.setMascotMessage('Isso aí! Já estamos na metade do caminho, continue assim! ✨');
      } else if (pct < 100) {
        crown.classList.add('hidden');
        mouth.setAttribute('d', 'M 86 166 Q 100 186 114 166'); // super sorriso
        this.setMascotMessage('Quase lá!! Falta muito pouquinho para completar o dia! 🌟');
      } else {
        crown.classList.remove('hidden');
        mouth.setAttribute('d', 'M 85 165 Q 100 190 115 165'); // boca radiante
        this.setMascotMessage('PARABÉNS!! 🎉 Meta batida com louvor! Você é nota 10!');
      }
    }

    pokeMascot() {
      // Animação de cutucar a gotinha
      const phrases = [
        'Hihi! Beba mais um golinho d\'água! 💧',
        'Estou aqui para cuidar de você no PC! 🥰',
        'Pele radiante e mente afiada precisam de água! ✨',
        'Gota a gota a gente bate a meta! 🚰',
        'Você é maravilhosa, não esquece de se hidratar! 💖',
      ];
      const randomMsg = phrases[Math.floor(Math.random() * phrases.length)];
      this.setMascotMessage(randomMsg);
      this.sound.playWaterDrop();

      // Piscar
      this.dom.mascotEyes.classList.add('blink');
      setTimeout(() => this.dom.mascotEyes.classList.remove('blink'), 300);
    }

    startMascotBlinking() {
      setInterval(() => {
        if (Math.random() > 0.4) {
          this.dom.mascotEyes.classList.add('blink');
          setTimeout(() => this.dom.mascotEyes.classList.remove('blink'), 250);
        }
      }, 3500);
    }

    renderTodayHistory() {
      const drinks = this.todayData.drinks;
      this.dom.drinksCount.textContent = drinks.length;

      if (drinks.length === 0) {
        this.dom.historyList.innerHTML = `
          <div class="empty-state">
            <span>🌱</span>
            <p>Nenhum gole registrado ainda hoje.<br>Beba seu primeiro copo para começar!</p>
          </div>
        `;
        return;
      }

      this.dom.historyList.innerHTML = drinks
        .map(
          (d) => `
        <div class="history-item">
          <div class="item-left">
            <span class="item-icon">💧</span>
            <div>
              <div class="item-amount">+${d.amount} ml</div>
              <div class="item-time">${d.label}</div>
            </div>
          </div>
          <div class="item-time">${d.time}</div>
        </div>
      `
        )
        .join('');
    }

    renderWeekChart() {
      const days = ['Dom', 'Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sáb'];
      const today = new Date();
      const weekData = [];
      let totalWeek = 0;

      for (let i = 6; i >= 0; i--) {
        const d = new Date();
        d.setDate(today.getDate() - i);
        const key = `water_log_${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
        const saved = localStorage.getItem(key);
        const intake = saved ? JSON.parse(saved).intake : 0;
        totalWeek += intake;

        weekData.push({
          dayLabel: i === 0 ? 'Hoje' : days[d.getDay()],
          intake: intake,
          isToday: i === 0,
        });
      }

      const maxIntake = Math.max(this.config.target, ...weekData.map((w) => w.intake), 2000);

      this.dom.weekChart.innerHTML = weekData
        .map((item, idx) => {
          const heightPct = Math.min(100, Math.round((item.intake / maxIntake) * 100));
          const achieved = item.intake >= this.config.target;
          return `
          <div class="chart-bar-col">
            <div class="chart-val">${item.intake > 0 ? `${(item.intake / 1000).toFixed(1)}L` : '0'}</div>
            <div class="chart-bar-wrap">
              <div class="chart-bar-fill ${achieved ? 'achieved' : ''}" data-target-height="${heightPct}%" style="height: 0%; transition-delay: ${idx * 45}ms;"></div>
            </div>
            <div class="chart-label" style="${item.isToday ? 'color: var(--primary); font-weight:800;' : ''}">${item.dayLabel}</div>
          </div>
        `;
        })
        .join('');

      // Animação cascata ao renderizar as barras da semana
      requestAnimationFrame(() => {
        requestAnimationFrame(() => {
          this.dom.weekChart.querySelectorAll('.chart-bar-fill').forEach((bar) => {
            bar.style.height = bar.dataset.targetHeight;
          });
        });
      });

      const avg = Math.round(totalWeek / 7);
      this.dom.weekSummary.innerHTML = `Média diária dos últimos 7 dias: <strong>${avg} ml</strong>`;
    }

    // --- Lembrete & Timer ---
    startTimer() {
      this.timerSeconds = this.config.interval * 60;
      this.updateTimerDisplay();

      if (this.timerInterval) clearInterval(this.timerInterval);

      this.timerInterval = setInterval(() => {
        if (this.isTimerPaused) return;

        if (this.timerSeconds > 0) {
          this.timerSeconds--;
          this.updateTimerDisplay();
        } else {
          this.triggerReminderAlert();
          this.resetTimer();
        }
      }, 1000);
    }

    resetTimer() {
      this.timerSeconds = this.config.interval * 60;
      this.updateTimerDisplay();
    }

    snoozeTimer(minutes = 5) {
      this.timerSeconds = minutes * 60;
      this.updateTimerDisplay();
    }

    toggleTimer() {
      this.isTimerPaused = !this.isTimerPaused;
      if (this.isTimerPaused) {
        this.dom.btnTimerToggle.innerHTML = '<span id="timerToggleIcon">▶️</span> Retomar';
        this.dom.btnTimerToggle.classList.replace('primary', 'secondary');
        this.showToast('Lembrete pausado ⏸️', '⏰');
      } else {
        this.dom.btnTimerToggle.innerHTML = '<span id="timerToggleIcon">⏸️</span> Pausar';
        this.dom.btnTimerToggle.classList.replace('secondary', 'primary');
        this.showToast('Lembrete retomado ▶️', '⏰');
      }
    }

    updateTimerDisplay() {
      const mins = Math.floor(this.timerSeconds / 60);
      const secs = this.timerSeconds % 60;
      this.dom.timerDisplay.textContent = `${String(mins).padStart(2, '0')}:${String(secs).padStart(2, '0')}`;
    }

    setIntervalMinutes(minutes) {
      this.config.interval = minutes;
      this.saveConfig();
      this.updateIntervalChips();
      this.resetTimer();
      this.showToast(`Lembrar a cada ${minutes} minutos!`, '⏰');
    }

    updateIntervalChips() {
      document.querySelectorAll('.interval-chips .chip').forEach((c) => {
        if (parseInt(c.dataset.min, 10) === this.config.interval) {
          c.classList.add('active');
        } else {
          c.classList.remove('active');
        }
      });
    }

    triggerReminderAlert() {
      // 1. Toca o chime suave
      this.sound.playReminderChime();

      // 2. Abre o modal visual suavemente
      const modal = this.dom.reminderAlertModal;
      modal.classList.remove('hidden', 'closing');

      // 3. Piscar o título na aba / barra de tarefas
      this.startFlashingTitle('🔔 HORA DA ÁGUA! 💧 Lumi');

      // 4. Notificação do Windows (Web Notification)
      if ('Notification' in window && Notification.permission === 'granted') {
        try {
          new Notification('Lumi: Hora de Beber Água! 💧', {
            body: 'Que tal uma pausa rápida para um copo de água fresquinha agora?',
            icon: 'data:image/svg+xml,<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100"><text y=".9em" font-size="90">💧</text></svg>',
            silent: !this.config.soundEnabled,
          });
        } catch (e) {
          console.warn('Erro ao disparar notificação:', e);
        }
      }
    }

    closeReminderAlert() {
      const modal = this.dom.reminderAlertModal;
      if (!modal || modal.classList.contains('hidden') || modal.classList.contains('closing')) return;
      modal.classList.add('closing');
      setTimeout(() => {
        modal.classList.add('hidden');
        modal.classList.remove('closing');
      }, 200);
      this.stopFlashingTitle();
    }

    startFlashingTitle(alertTitle) {
      this.stopFlashingTitle();
      let flag = false;
      this.flashTitleInterval = setInterval(() => {
        document.title = flag ? alertTitle : '💧 Beba Água!';
        flag = !flag;
      }, 1000);
    }

    stopFlashingTitle() {
      if (this.flashTitleInterval) {
        clearInterval(this.flashTitleInterval);
        this.flashTitleInterval = null;
      }
      document.title = this.originalTitle;
    }

    // --- Notificações do Sistema ---
    checkNotificationStatus() {
      if (!('Notification' in window)) {
        this.dom.notificationStatusText.textContent = 'Notificações não suportadas neste navegador.';
        this.dom.btnToggleNotifications.style.display = 'none';
        return;
      }

      if (Notification.permission === 'granted') {
        this.dom.notificationStatusText.textContent = '✅ Notificações do Windows ativadas com sucesso!';
        this.dom.btnToggleNotifications.textContent = 'Ativado ✓';
        this.dom.btnToggleNotifications.classList.replace('primary', 'secondary');
      } else if (Notification.permission === 'denied') {
        this.dom.notificationStatusText.textContent = '❌ Permissão bloqueada nas configurações do sistema/navegador.';
        this.dom.btnToggleNotifications.textContent = 'Bloqueado';
      }
    }

    async requestNotifications() {
      if (!('Notification' in window)) {
        alert('Este ambiente não suporta notificações de sistema.');
        return;
      }
      const perm = await Notification.requestPermission();
      this.checkNotificationStatus();
      if (perm === 'granted') {
        new Notification('Lembrete de Água ativado! 💧', {
          body: 'Pronto! Você receberá lembretes diretamente na sua tela do PC.',
        });
        this.showToast('Notificações ativadas no Windows!', '🔔');
      }
    }

    // --- Som ---
    toggleSound() {
      this.config.soundEnabled = !this.config.soundEnabled;
      this.sound.enabled = this.config.soundEnabled;
      this.saveConfig();
      this.updateSoundIcon();
      if (this.sound.enabled) {
        this.sound.playWaterDrop();
        this.showToast('Sons ativados 🔊', '🎵');
      } else {
        this.showToast('Sons silenciados 🔇', '🎵');
      }
    }

    updateSoundIcon() {
      this.dom.soundIcon.textContent = this.config.soundEnabled ? '🔊' : '🔇';
    }

    // --- Configurações & Reset ---
    openSettings() {
      this.dom.inputTarget.value = this.config.target;
      this.checkNotificationStatus();
      const modal = this.dom.settingsModal;
      modal.classList.remove('hidden', 'closing');
    }

    closeSettings() {
      const modal = this.dom.settingsModal;
      if (!modal || modal.classList.contains('hidden') || modal.classList.contains('closing')) return;
      modal.classList.add('closing');
      setTimeout(() => {
        modal.classList.add('hidden');
        modal.classList.remove('closing');
      }, 200);
    }

    saveSettings() {
      const target = parseInt(this.dom.inputTarget.value, 10);
      if (target && target >= 500 && target <= 8000) {
        this.config.target = target;
        this.saveConfig();
        this.render();
        this.closeSettings();
        this.showToast(`Meta salva: ${target} ml por dia! ✨`, '⚙️');
      } else {
        alert('Por favor, informe uma meta válida entre 500 ml e 8000 ml.');
      }
    }

    resetToday() {
      if (confirm('Tem certeza que deseja zerar o contador de hoje?')) {
        this.todayData = { intake: 0, drinks: [] };
        this.saveTodayData();
        this.render();
        this.closeSettings();
        this.showToast('Contador de hoje zerado.', '🌱');
      }
    }

    rotateTip() {
      let index = 0;
      setInterval(() => {
        index = (index + 1) % HEALTH_TIPS.length;
        this.dom.tipText.textContent = HEALTH_TIPS[index];
      }, 20000);
    }
  }

  // Iniciar a aplicação quando o DOM estiver pronto
  document.addEventListener('DOMContentLoaded', () => {
    window.waterApp = new WaterApp();
  });
})();
