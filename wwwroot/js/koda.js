/*
 * Capa de vida de Koda (vanilla JS, sin librerías).
 *   1. Koda reactivo   -> cambia de expresión según lo que pasa (chat, logros).
 *   2. Micro-interacciones -> rebote al hacer click (Web Animations API, se
 *      compone con la animación idle sin pelearse por el transform).
 *   3. Aura de partículas -> canvas de píxeles neón azules detrás del sprite.
 *
 * Todo respeta prefers-reduced-motion: sin movimiento ni partículas.
 */
(function () {
    'use strict';

    var BASE = '/images/koda/';
    // Caras (mismo encuadre entre sí, para que el swap no salte de tamaño).
    var FACES = {
        neutral: 'koda-logo.png',
        pensativo: 'koda-pensativo.png',
        motivado: 'koda-motivado.png',
        sorprendido: 'koda-sorprendido.png',
        enfocado: 'koda-enfocado.png'
    };
    var REDUCE = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    // Precarga para que el cambio de expresión no parpadee.
    for (var k in FACES) { var pre = new Image(); pre.src = BASE + FACES[k]; }

    // ---------------------------------------------------------------- Reactivo
    var chatTimer = null;
    function chatSprite() { return document.querySelector('[data-koda="chat"]'); }

    var Koda = {
        setChat: function (state) {
            var el = chatSprite();
            if (!el) return;
            el.src = BASE + (FACES[state] || FACES.neutral);
        },
        chatThinking: function () { clearTimeout(chatTimer); this.setChat('pensativo'); },
        chatAnswered: function () {
            clearTimeout(chatTimer);
            this.setChat('motivado');
            var self = this;
            chatTimer = setTimeout(function () { self.setChat('neutral'); }, 3500);
        },
        chatError: function () {
            clearTimeout(chatTimer);
            this.setChat('sorprendido');
            var self = this;
            chatTimer = setTimeout(function () { self.setChat('neutral'); }, 3500);
        },
        // Rebote de una sola vez, sumado (composite:'add') sobre el idle.
        pop: function (el) {
            if (REDUCE || !el || !el.animate) return;
            el.animate(
                [{ transform: 'translateY(0)' }, { transform: 'translateY(-16px)' }, { transform: 'translateY(0)' }],
                { duration: 460, easing: 'cubic-bezier(.34,1.56,.64,1)', composite: 'add' }
            );
        }
    };
    window.Koda = Koda;

    function init() {
        // Click en cualquier Koda -> rebote; si es el del chat, sonríe un momento.
        var clickables = document.querySelectorAll('.koda-interactive, [data-koda]');
        Array.prototype.forEach.call(clickables, function (el) {
            el.style.cursor = 'pointer';
            el.addEventListener('click', function () {
                Koda.pop(el);
                if (el.getAttribute('data-koda') === 'chat') {
                    clearTimeout(chatTimer);
                    Koda.setChat('motivado');
                    chatTimer = setTimeout(function () { Koda.setChat('neutral'); }, 1600);
                }
            });
        });

        // Koda celebra al entrar a una pantalla de logros/celebración.
        var fest = document.querySelector('[data-koda-celebra]');
        if (fest) setTimeout(function () { Koda.pop(fest); }, 350);

        // Aura de partículas.
        if (!REDUCE) {
            var auras = document.querySelectorAll('[data-koda-aura]');
            Array.prototype.forEach.call(auras, mountAura);
        }
    }

    // -------------------------------------------------------------- Partículas
    function mountAura(sprite) {
        var host = sprite.parentElement;
        if (!host) return;
        if (getComputedStyle(host).position === 'static') host.style.position = 'relative';
        if (getComputedStyle(sprite).position === 'static') sprite.style.position = 'relative';
        sprite.style.zIndex = '1';

        var canvas = document.createElement('canvas');
        canvas.setAttribute('aria-hidden', 'true');
        var s = canvas.style;
        s.position = 'absolute';
        s.pointerEvents = 'none'; s.zIndex = '0';
        host.insertBefore(canvas, host.firstChild);

        var ctx = canvas.getContext('2d');
        var PAD = 26;
        var W = 0, H = 0, dpr = 1;
        // El canvas se ciñe a la caja del sprite (con margen), no a toda la tarjeta.
        function resize() {
            dpr = Math.min(window.devicePixelRatio || 1, 2);
            W = Math.max(1, sprite.offsetWidth + PAD * 2);
            H = Math.max(1, sprite.offsetHeight + PAD * 2);
            s.left = (sprite.offsetLeft - PAD) + 'px';
            s.top = (sprite.offsetTop - PAD) + 'px';
            s.width = W + 'px'; s.height = H + 'px';
            canvas.width = W * dpr; canvas.height = H * dpr;
            ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
        }
        resize();
        window.addEventListener('resize', resize);
        // El sprite puede llegar después (imagen sin cachear) y cambiar de tamaño.
        if (!sprite.complete) sprite.addEventListener('load', resize);

        function spawn(atBottom) {
            return {
                x: Math.random() * W,
                y: atBottom ? H * (0.85 + Math.random() * 0.15) : H * (0.4 + Math.random() * 0.6),
                vy: 0.15 + Math.random() * 0.55,
                size: Math.random() < 0.5 ? 2 : 3,
                seed: Math.random() * 100,
                life: 0,
                max: 110 + Math.random() * 130,
                cool: Math.random() < 0.5
            };
        }
        var parts = [];
        for (var i = 0; i < 24; i++) parts.push(spawn(false));

        function frame() {
            ctx.clearRect(0, 0, W, H);
            ctx.globalCompositeOperation = 'lighter';
            for (var j = 0; j < parts.length; j++) {
                var p = parts[j];
                p.life++;
                p.y -= p.vy;
                p.x += Math.sin((p.life + p.seed) / 22) * 0.35;
                var t = p.life / p.max;
                var a = t < 0.2 ? t / 0.2 : (1 - (t - 0.2) / 0.8);
                if (p.life >= p.max || p.y < -4) { parts[j] = spawn(true); continue; }
                var col = p.cool ? '108,155,255' : '47,107,255';
                ctx.fillStyle = 'rgba(' + col + ',' + (a * 0.8).toFixed(3) + ')';
                ctx.fillRect(p.x | 0, p.y | 0, p.size, p.size);
            }
            requestAnimationFrame(frame);
        }
        requestAnimationFrame(frame);
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
