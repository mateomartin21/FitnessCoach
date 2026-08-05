/*
 * Aviso de que la pantalla está cambiando.
 *
 * La app es MVC: cada sección es una navegación real, así que entre el toque y la
 * pantalla nueva hay una espera que depende del servidor. En Render, con la
 * instancia dormida, son segundos. Durante esa espera no pasaba NADA: el usuario
 * tocaba y se quedaba mirando la pantalla vieja, sin saber si su toque contó.
 *
 * Esto pinta una barra fina arriba en cuanto se dispara la navegación. No acelera
 * nada; hace visible que el toque se registró, que es lo que faltaba.
 */
(function () {
    'use strict';

    var barra = null;

    function mostrar() {
        if (barra) return;
        barra = document.createElement('div');
        barra.className = 'fc-cargando';
        barra.setAttribute('aria-hidden', 'true');
        document.body.appendChild(barra);
        // Se fuerza un reflow antes de la clase que la hace avanzar: sin esto el
        // navegador junta los dos estados y no hay transicion que ver.
        void barra.offsetWidth;
        barra.classList.add('fc-cargando-avanza');
    }

    function ocultar() {
        if (!barra) return;
        barra.remove();
        barra = null;
    }

    // Solo los clicks que de verdad van a navegar. Un enlace que abre en otra
    // pestana, un ancla de la misma pagina o un boton de Bootstrap no cambian de
    // pantalla, y dejarian la barra encendida para siempre.
    function esNavegacionReal(evento, enlace) {
        if (evento.defaultPrevented) return false;
        if (evento.button !== 0 || evento.metaKey || evento.ctrlKey || evento.shiftKey || evento.altKey) return false;
        if (enlace.target && enlace.target !== '_self') return false;
        if (enlace.hasAttribute('download') || enlace.hasAttribute('data-bs-toggle')) return false;

        var destino = enlace.getAttribute('href') || '';
        if (!destino || destino.charAt(0) === '#') return false;
        if (/^(javascript|mailto|tel):/i.test(destino)) return false;

        var url = new URL(enlace.href, location.href);
        if (url.origin !== location.origin) return false;
        // Mismo documento, distinto ancla: no hay carga que esperar.
        return url.href.split('#')[0] !== location.href.split('#')[0];
    }

    // En fase de burbujeo, NO de captura. Varios formularios llevan
    // onsubmit="return confirm(...)": en captura la barra se encendia antes de
    // preguntar, y si el usuario cancelaba quedaba prendida para siempre. Acá el
    // handler del propio formulario ya corrio, asi que defaultPrevented dice la verdad.
    document.addEventListener('click', function (evento) {
        var enlace = evento.target.closest ? evento.target.closest('a[href]') : null;
        if (enlace && esNavegacionReal(evento, enlace)) mostrar();
    });

    document.addEventListener('submit', function (evento) {
        var formulario = evento.target;
        if (!evento.defaultPrevented && formulario && formulario.tagName === 'FORM') mostrar();
    });

    // Volver con el boton "atras" puede devolver la pagina desde la cache del
    // navegador con la barra todavia puesta.
    window.addEventListener('pageshow', ocultar);
    window.addEventListener('pagehide', ocultar);
})();
