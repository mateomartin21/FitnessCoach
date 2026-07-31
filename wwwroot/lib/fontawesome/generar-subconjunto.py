# -*- coding: utf-8 -*-
"""Arma un Font Awesome a medida: solo los iconos que la app usa de verdad.

Lee el CSS oficial para sacar el codepoint de cada clase, recorta las fuentes con
fontTools y escribe un CSS propio con unicamente esas reglas.
"""
import io, os, re, sys
from fontTools import subset
from fontTools.ttLib import TTFont

AQUI = os.path.dirname(os.path.abspath(__file__))
SALIDA = sys.argv[1]                      # wwwroot/lib/fontawesome
USADOS = [l.strip() for l in io.open(sys.argv[2], encoding='utf-8') if l.strip()]

css = io.open(os.path.join(AQUI, 'all.min.css'), encoding='utf-8').read()

# .fa-house:before{content:"\f015"}  — y los alias comparten regla: .fa-a:before,.fa-b:before{...}
mapa = {}
for m in re.finditer(r'((?:\.fa-[a-z0-9-]+:{1,2}before\s*,?\s*)+)\{\s*content\s*:\s*"\\([0-9a-f]+)"', css):
    for clase in re.findall(r'\.fa-([a-z0-9-]+):{1,2}before', m.group(1)):
        mapa['fa-' + clase] = int(m.group(2), 16)

faltantes = [i for i in USADOS if i not in mapa]
if faltantes:
    print('SIN CODEPOINT (revisar):', ', '.join(faltantes))

# fa-youtube es la unica marca; el resto son solidos.
MARCAS = {'fa-youtube'}
grupos = {
    'fa-solid-900':  sorted({mapa[i] for i in USADOS if i in mapa and i not in MARCAS}),
    'fa-brands-400': sorted({mapa[i] for i in USADOS if i in mapa and i in MARCAS}),
}

os.makedirs(SALIDA, exist_ok=True)
for nombre, puntos in grupos.items():
    origen = os.path.join(AQUI, nombre + '.woff2')
    destino = os.path.join(SALIDA, nombre + '.woff2')

    fuente = TTFont(origen)
    opciones = subset.Options()
    opciones.flavor = 'woff2'
    opciones.desubroutinize = True
    opciones.layout_features = []
    opciones.notdef_outline = True
    subsetter = subset.Subsetter(options=opciones)
    subsetter.populate(unicodes=puntos)
    subsetter.subset(fuente)
    fuente.flavor = 'woff2'
    fuente.save(destino)

    print('%-14s %3d iconos  %6d -> %5d bytes' % (
        nombre, len(puntos), os.path.getsize(origen), os.path.getsize(destino)))

# CSS propio: solo lo que la app necesita para que <i class="fas fa-x"> siga funcionando.
lineas = [
    '/* Font Awesome Free 6.4.0 — subconjunto propio con los %d iconos que usa la app.' % len(USADOS),
    '   Generado, no editar a mano. Autohospedado para no depender de un CDN (D-34).',
    '   Iconos: CC BY 4.0 · Fuentes: SIL OFL 1.1 · https://fontawesome.com */',
    '',
    '@font-face {',
    '    font-family: "Font Awesome 6 Free";',
    '    src: url("fa-solid-900.woff2") format("woff2");',
    '    font-weight: 900;',
    '    font-style: normal;',
    '    font-display: block;',
    '}',
    '',
    '@font-face {',
    '    font-family: "Font Awesome 6 Brands";',
    '    src: url("fa-brands-400.woff2") format("woff2");',
    '    font-weight: 400;',
    '    font-style: normal;',
    '    font-display: block;',
    '}',
    '',
    '.fas, .fab {',
    '    -moz-osx-font-smoothing: grayscale;',
    '    -webkit-font-smoothing: antialiased;',
    '    display: var(--fa-display, inline-block);',
    '    font-style: normal;',
    '    font-variant: normal;',
    '    line-height: 1;',
    '    text-rendering: auto;',
    '}',
    '',
    '.fas { font-family: "Font Awesome 6 Free"; font-weight: 900; }',
    '.fab { font-family: "Font Awesome 6 Brands"; font-weight: 400; }',
    '',
    '/* Tamanos y utilidades que la app usa */',
    '.fa-lg { font-size: 1.25em; line-height: .05em; vertical-align: -.075em; }',
    '.fa-2x { font-size: 2em; }',
    '.fa-3x { font-size: 3em; }',
    '.fa-fw { text-align: center; width: 1.25em; }',
    '.fa-spin { animation: fa-spin 2s infinite linear; }',
    '@keyframes fa-spin { 0% { transform: rotate(0deg); } 100% { transform: rotate(360deg); } }',
    '@media (prefers-reduced-motion: reduce) { .fa-spin { animation: none; } }',
    '',
    '/* Los %d iconos */' % len(USADOS),
]
for icono in sorted(USADOS):
    if icono in mapa:
        lineas.append('.%s::before { content: "\\%x"; }' % (icono, mapa[icono]))

io.open(os.path.join(SALIDA, 'fontawesome-subset.css'), 'w', encoding='utf-8').write('\n'.join(lineas) + '\n')
print('css:', os.path.getsize(os.path.join(SALIDA, 'fontawesome-subset.css')), 'bytes')
