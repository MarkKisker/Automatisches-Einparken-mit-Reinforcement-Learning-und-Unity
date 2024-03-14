# Automatisches-Einparken-mit-Reinforcement-Learning-und-Unity
## Einleitung

Willkommen zum Projekt "Automatisches Einparken mit Reinforcement Learning und Unity"! Dieses Repository befasst sich mit der Entwicklung eines automatisierten Einparksystems mithilfe von Reinforcement Learning, auf Basis der [MlAgents Bibliothek](https://github.com/Unity-Technologies/ml-agents), und der Unity-Engine zur Simulation der Umgebung. Das Ziel dieses Projekts ist es, ein intelligentes Einparksystem zu entwickeln, das in der Lage ist, Fahrzeuge autonom und effizient in Parklücken zu manövrieren.

## Installationsanleitung

Um das Projekt lokal auf Ihrem System einzurichten, folgen Sie bitte diesen Anweisungen:
1. **Conda python setup**
   ```bash
   conda create -n mlagents python=3.9.13 && conda activate mlagents
2. **Klonen des Repositories:**
   ```bash
   git clone https://github.com/MarkKisker/Automatisches-Einparken-mit-Reinforcement-Learning-und-Unity.git
3. **Klonen des MlAgents Repositories in der Version 20 in das Projekt Repositorie**
   ```bash
   git clone --branch release_20 https://github.com/Unity-Technologies/ml-agents.git
4. **Installation von PyTorch**
    ```bash
   pip3 install torch~=1.13.1 -f https://download.pytorch.org/whl/torch_stable.html
5. **Installation von der MlAgents Bibliothek**
   ```bash
   cd /path/to/ml-agents
   python -m pip install ./ml-agents-envs
   python -m pip install ./ml-agents
6. **Installation verifizieren**
   Wenn alles erfolgreich war, sollte folgender Befehl ausführbar sein:
   ```bash
   mlagents-learn --help

## Ausführen des Projekt Ergbniss
1. Navigation in das Projektverzeichniss
2. Aktivierung der conda Umgebung
3. Folgende Befehle können nun ausgeführt werden:
   ```bash
   mlagents-learn config\AutoParking_configPPO.yaml --run-id=FinalResult --inference --env=Build --num-envs=1 --width=1280 --height=720 --resume
   mlagents-learn config\AutoParking_configPPO.yaml --run-id=FinalResult --env=Build --num-envs=1 --width=1280 --height=720 --resume
  Der erste Befehl führt das Projekt im Inferenzmodus aus, wodurch der Agent nicht weiter lernt. Der zweite Befehl setzt das Training fort.
  Um die Ergebnisse einzusehen, navigieren Sie bitte zunächst zum Projektverzeichnis und stellen sicher, dass die ML-Agents-Umgebung aktiviert ist. Anschließend können Sie den folgenden       Befehl verwenden.
  ```bash
  tensorboard logdir=results
  
