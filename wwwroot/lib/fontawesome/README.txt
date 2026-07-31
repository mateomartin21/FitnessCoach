Font Awesome Free 6.4.0 — subconjunto propio (D-34)
====================================================

Estos archivos NO son el paquete oficial: son un subconjunto generado con solo los
iconos que la app usa. El paquete completo pesa ~1.5 MB y el CSS+fuentes del CDN
~360 KB; esto pesa 12 KB.

  fa-solid-900.woff2       64 iconos
  fa-brands-400.woff2       1 icono (fa-youtube)
  fontawesome-subset.css   las 68 reglas .fa-*::before y nada mas

Por que autohospedado
---------------------
Era la unica libreria de front que venia de un CDN (cdnjs). Si el CDN no responde o
esta bloqueado, la app se queda SIN NINGUN icono. Ahora no se le pide nada a terceros.

Como regenerarlo si se agregan iconos
-------------------------------------
1. Listar los iconos usados:

   grep -rhoE "fa-[a-z0-9-]+" Views/ wwwroot/js/ wwwroot/css/ \
     | sort -u | grep -v "^fa-lg$\|^fa-2x$\|^fa-3x$\|^fa-spin$\|^fa-fw$" > iconos.txt

2. Bajar de cdnjs (version 6.4.0) `css/all.min.css`, `webfonts/fa-solid-900.woff2`
   y `webfonts/fa-brands-400.woff2` a una carpeta temporal.

3. Correr el script de subconjunto (requiere `fonttools` y `brotli`), que lee el CSS
   oficial para sacar el codepoint de cada clase, recorta las fuentes y escribe el CSS.

El marcado no cambia: `<i class="fas fa-x">` sigue funcionando igual que con el CDN.

Licencias
---------
Iconos: CC BY 4.0 · Fuentes: SIL OFL 1.1 · Codigo: MIT — https://fontawesome.com
