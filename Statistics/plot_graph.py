'''
Python script to plot log statistics
Pandas and matplotlib libraries are required
'''
import pandas as pd
import matplotlib.pyplot as plt
import numpy as np

# Here set expected log path
data = pd.read_csv('Statistics/lockdown_log.txt', sep='\t', engine="python")
# Due to long log file, parse it with fixed offset
data = pd.DataFrame(data).iloc[::100, :]

plt.style.use('ggplot')
plt.xlabel('Time')
plt.ylabel("Number")

# time parsing for xtick feature
k = 10

xoffset = []
xlabel = []
for i in data['MinutesPassed'][::k]:
    # Transform minutes time into more readable format
    value = int(i)
    ndays = int(value/1440)
    nhours = int((value % 1440)/60)
    minutes = (value % 1440) % 60
    date = f"{ndays}D {nhours}H {minutes}m"
    xoffset.append(i)
    xlabel.append(date)


# plot everything
plt.plot(data['MinutesPassed'], data['Population'], label="Total Population")
plt.plot(data['MinutesPassed'], data['Exposed'], label="Exposed")
plt.plot(data['MinutesPassed'], data['TotalExposed'], label="Total Exposed")
plt.plot(data['MinutesPassed'], data['Symptomatic'], label="Symptomatic")
plt.plot(data['MinutesPassed'], data['Asymptomatic'], label="Asymptomatic")
plt.plot(data['MinutesPassed'], data['Death'], label="Death")
plt.plot(data['MinutesPassed'], data['Recovered'], label="Recovered")
plt.plot(data['MinutesPassed'], data['TotalRecovered'],
         label="Total Recovered")
# set title
plt.title("Data analysis with lockdown")
plt.xticks(xoffset, xlabel, rotation='vertical')
plt.legend()
plt.grid(True)
plt.show()
