if "%1"=="" goto end

wget -O "%1_SecurityPrices.csv" "http://real-chart.finance.yahoo.com/table.csv?s=%1.TO&a=01&b=1&c=2014&d=11&e=2&f=2016&g=d&ignore=.csv"
wget -O "%1_SecurityDividends.csv" "http://real-chart.finance.yahoo.com/table.csv?s=%1.TO&a=01&b=1&c=2014&d=11&e=2&f=2016&g=v&ignore=.csv"

:end

