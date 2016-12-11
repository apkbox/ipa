if "%1"=="" goto end

wget -O "%1_SecurityPrices.csv" "http://real-chart.finance.yahoo.com/table.csv?s=%1&a=01&b=1&c=2005&d=12&e=7&f=2016&g=d&ignore=.csv"
wget -O "%1_SecurityDividends.csv" "http://real-chart.finance.yahoo.com/table.csv?s=%1&a=01&b=1&c=2005&d=12&e=7&f=2016&g=v&ignore=.csv"

:end

