import matplotlib.pyplot as plt
from tkinter import filedialog


def getAverageRun(runs: []):
    sumRun = []
    avgRun = []

    i = 0
    for s in runs:
        j = 0
        for w in s:
            if len(sumRun) == 0 or len(sumRun) == j:
                sumRun.append(w)
            elif len(sumRun) > j:
                sumRun[j] += w
            j += 1
        i += 1

    for i in sumRun:
        avgRun.append(i / len(runs))

    return avgRun


runs = []


while input("p for plot, a for adding a new run") != "p":
    filenames = filedialog.askopenfilenames()
     
    datas = []
    for filename in filenames:
        file = open(filename)
        datas.append(file.readline())
    
    for data in datas:
        winrates = []

        avgAmount = 100
        counter = 0

        wins = 0

        for c in data:
            counter += 1
            if c == 'w':
                wins += 1
    
            if (counter % avgAmount) == 0:
                winrates.append(wins / avgAmount)
                wins = 0
    
        runs.append(winrates)


avg = getAverageRun(runs)
plt.ylabel("Winrate over "+ str(avgAmount) +" episodes")
plt.xlabel("Episodes (in 100 increments)")
plt.plot([i for i in range(0, len(avg))],avg)
plt.ylim(0, 1)
plt.show()




            

