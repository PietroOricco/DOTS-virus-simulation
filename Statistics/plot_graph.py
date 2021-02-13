'''
Python script to plot log statistics
Pandas and matplotlib libraries are required
'''
import pandas as pd
import matplotlib.pyplot as plt

data = pd.read_csv('log.txt', sep='\s+')
data = pd.DataFrame(data).iloc[::100, :]

plt.style.use('ggplot')
plt.xlabel('Time')
plt.ylabel("Number")

xlabel = []
for i in data['MinutesPassed'][::10]:
    integer = int(i.split(",")[0])
    ndays = int(integer/1440)
    nhours = int((integer % 1440)/60)
    minutes = (integer % 1440) % 60
    date = f"{ndays}D {nhours}H {minutes}m"
    xlabel.append(date)

plt.xticks(range(0, len(data['MinutesPassed']), 10),
           xlabel, rotation="vertical")

plt.plot(data['MinutesPassed'], data['Population'], label="Total Population")
plt.plot(data['MinutesPassed'], data['Exposed'], label="Exposed")
plt.plot(data['MinutesPassed'], data['TotalExposed'], label="Total Exposed")
plt.plot(data['MinutesPassed'], data['Symptomatic'], label="Symptomatic")
plt.plot(data['MinutesPassed'], data['Asymptomatic'], label="Asymptomatic")
plt.plot(data['MinutesPassed'], data['Death'], label="Death")
plt.plot(data['MinutesPassed'], data['Recovered'], label="Recovered")
plt.plot(data['MinutesPassed'], data['TotalRecovered'],
         label="Total Recovered")
plt.title("Data analysis without lockdown")
plt.legend()
plt.grid(True)
plt.show()
