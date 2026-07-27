/*
 * Capa gráfica de los logros: dibuja cada logro como una medalla pixel (moneda
 * octagonal con bisel) en el color de su categoría, con un glifo dentro. Las
 * desbloqueadas brillan y laten; las bloqueadas van en gris. Todo en canvas,
 * sin imágenes que descargar. Reemplaza a los emojis.
 */
(function () {
    'use strict';

    var INK = [5, 7, 14];
    var WHITE = [234, 240, 251];

    // Glifos 11x11 ('#' = pixel encendido).
    var GLYPHS = {
        dumbbell: [
            "...........",
            ".##.....##.",
            ".##.....##.",
            ".##.....##.",
            ".#########.",
            ".#########.",
            ".#########.",
            ".##.....##.",
            ".##.....##.",
            ".##.....##.",
            "..........."],
        bolt: [
            ".....###...",
            "....###....",
            "...###.....",
            "..####.....",
            ".#######...",
            "...####....",
            "...###.....",
            "..###......",
            ".###.......",
            ".##........",
            "..........."],
        arrow: [
            ".....#.....",
            "....###....",
            "...#####...",
            "..#######..",
            ".#########.",
            "....###....",
            "....###....",
            "....###....",
            "....###....",
            "....###....",
            "..........."],
        ring: [
            "...#####...",
            "..##...##..",
            ".##.....##.",
            ".#.......#.",
            ".#.......#.",
            ".#.......#.",
            ".#.......#.",
            ".##.....##.",
            "..##...##..",
            "...#####...",
            "..........."],
        list: [
            "...........",
            ".##.#####..",
            ".##.#####..",
            "...........",
            ".##.#####..",
            ".##.#####..",
            "...........",
            ".##.#####..",
            ".##.#####..",
            "...........",
            "..........."],
        target: [
            "...........",
            "..#######..",
            "..#.....#..",
            "..#.###.#..",
            "..#.#.#.#..",
            "..#.###.#..",
            "..#.....#..",
            "..#######..",
            "...........",
            "...........",
            "..........."],
        spark: [
            ".....#.....",
            "....###....",
            "...#####...",
            "..#######..",
            ".#########.",
            "..#######..",
            "...#####...",
            "....###....",
            ".....#.....",
            "...........",
            "..........."]
    };

    // logro id -> [glifo, color de categoría]
    var MAP = {
        'primer-entreno': ['dumbbell', '#2f6bff'],
        'diez-entrenos': ['dumbbell', '#2f6bff'],
        'cincuenta-entrenos': ['dumbbell', '#2f6bff'],
        'racha-3': ['bolt', '#f6a723'],
        'racha-7': ['bolt', '#f6a723'],
        'racha-30': ['bolt', '#f6a723'],
        'primer-record': ['arrow', '#27d17c'],
        'diez-records': ['arrow', '#27d17c'],
        'primer-peso': ['ring', '#9b6dff'],
        'diario-7': ['list', '#16c8b4'],
        'con-objetivo': ['target', '#ffcf3f'],
        'semana-de-fuego': ['spark', '#ef4655']
    };
    var DEFAULT = ['spark', '#2f6bff'];

    function hx(h) { h = h.replace('#', ''); return [parseInt(h.slice(0, 2), 16), parseInt(h.slice(2, 4), 16), parseInt(h.slice(4, 6), 16)]; }
    function mix(a, b, t) { return [Math.round(a[0] + (b[0] - a[0]) * t), Math.round(a[1] + (b[1] - a[1]) * t), Math.round(a[2] + (b[2] - a[2]) * t)]; }
    function rgb(c, a) { return 'rgba(' + c[0] + ',' + c[1] + ',' + c[2] + ',' + (a === undefined ? 1 : a) + ')'; }

    var S = 64, M = 7, C = 14, GP = 4;

    function draw(canvas, glyphName, colorHex, unlocked) {
        var dpr = Math.min(window.devicePixelRatio || 1, 2);
        canvas.width = S * dpr; canvas.height = S * dpr;
        canvas.style.width = S + 'px'; canvas.style.height = S + 'px';
        var ctx = canvas.getContext('2d');
        ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
        ctx.imageSmoothingEnabled = false;
        ctx.clearRect(0, 0, S, S);

        var base = hx(colorHex);
        var col = unlocked ? base : (function () { var g = Math.round((base[0] + base[1] + base[2]) / 3); return [g, g, g]; })();

        // Octágono (cuadrado con esquinas cortadas).
        var pts = [
            [M + C, M], [S - M - C, M], [S - M, M + C], [S - M, S - M - C],
            [S - M - C, S - M], [M + C, S - M], [M, S - M - C], [M, M + C]
        ];
        ctx.beginPath();
        ctx.moveTo(pts[0][0], pts[0][1]);
        for (var i = 1; i < pts.length; i++) ctx.lineTo(pts[i][0], pts[i][1]);
        ctx.closePath();
        ctx.fillStyle = rgb(col);
        ctx.fill();
        ctx.lineJoin = 'miter';
        ctx.lineWidth = 3;
        ctx.strokeStyle = rgb(INK);
        ctx.stroke();

        // Bisel: luz arriba-izquierda, sombra abajo-derecha.
        var hi = mix(col, WHITE, 0.35), sh = mix(col, INK, 0.35);
        ctx.lineWidth = 2;
        ctx.strokeStyle = rgb(hi);
        line(ctx, M + C, M + 3, S - M - C, M + 3);
        line(ctx, M + 3, M + C, M + 3, S - M - C);
        ctx.strokeStyle = rgb(sh);
        line(ctx, M + C, S - M - 3, S - M - C, S - M - 3);
        line(ctx, S - M - 3, M + C, S - M - 3, S - M - C);

        // Glifo centrado.
        var grid = GLYPHS[glyphName] || GLYPHS.spark;
        var ox = Math.round((S - 11 * GP) / 2), oy = Math.round((S - 11 * GP) / 2);
        ctx.fillStyle = unlocked ? rgb(WHITE) : rgb([205, 205, 205]);
        for (var y = 0; y < grid.length; y++) {
            var row = grid[y];
            for (var x = 0; x < row.length; x++) {
                if (row.charAt(x) === '#') ctx.fillRect(ox + x * GP, oy + y * GP, GP, GP);
            }
        }
    }

    function line(ctx, x1, y1, x2, y2) { ctx.beginPath(); ctx.moveTo(x1, y1); ctx.lineTo(x2, y2); ctx.stroke(); }

    function init() {
        var medals = document.querySelectorAll('canvas.logro-medal');
        Array.prototype.forEach.call(medals, function (cv) {
            var id = cv.getAttribute('data-logro');
            var unlocked = cv.getAttribute('data-unlocked') === '1';
            var conf = MAP[id] || DEFAULT;
            draw(cv, conf[0], conf[1], unlocked);
            if (unlocked) {
                cv.classList.add('is-unlocked');
                var c = hx(conf[1]);
                cv.style.filter = 'drop-shadow(0 0 5px ' + rgb(c, 0.85) + ') drop-shadow(0 0 11px ' + rgb(c, 0.45) + ')';
            }
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
