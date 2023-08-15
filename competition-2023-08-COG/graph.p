set output 'graph.svg'
set terminal svg enhanced font 'monospace,13' background rgb 'white' size 864,600
set grid mytics xtics lc rgb "gray" lw 1
set grid mytics ytics lc rgb "gray" lw 1
set key bottom outside center horizontal samplen 2 spacing 0.75 width -1
set datafile separator ';'
set xrange [0:200]
set yrange [0:100]
set ytics 10
unset border

# Colors based on rating.
set linetype  1 lc rgb hsv2rgb( 4.0 / 9.0, 1, 1)
set linetype  2 lc rgb hsv2rgb( 1.0 / 9.0, 1, 1)
set linetype  3 lc rgb hsv2rgb( 0.0 / 9.0, 1, 1)
set linetype  4 lc rgb hsv2rgb( 5.0 / 9.0, 1, 1)
set linetype  5 lc rgb hsv2rgb( 3.0 / 9.0, 1, 1)
set linetype  6 lc rgb hsv2rgb( 2.0 / 9.0, 1, 1)
set linetype  7 lc rgb hsv2rgb( 7.0 / 9.0, 1, 1)
set linetype  8 lc rgb hsv2rgb( 6.0 / 9.0, 1, 1)

plot for [col=1:8] 'graph.data' using 0:col with lines lw 3 title columnhead
