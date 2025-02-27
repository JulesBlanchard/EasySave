# EasySave Documentation
---

## Table des Matières

- [Introduction](#introduction)
- [Fonctionnalités](#fonctionnalités)
- [Architecture](#architecture)
    - [Vue d'ensemble](#vue-densemble)
    - [Diagramme de Classes](#diagramme-de-classes)
    - [Diagramme de Séquence](#diagramme-de-séquence)
- [Installation et Configuration](#installation-et-configuration)
    - [Prérequis](#prérequis)
    - [Installation](#installation)
    - [Configuration Dynamique](#configuration-dynamique)
- [Utilisation de l'Application](#utilisation-de-lapplication)
    - [Mode Interactif](#mode-interactif)
    - [Lancement en Ligne de Commande](#lancement-en-ligne-de-commande)
    - [Exemples d'Utilisation](#exemples-dutilisation)
- [Modules et Composants](#modules-et-composants)
    - [Gestion des Backups](#gestion-des-backups)
    - [Stratégies de Sauvegarde](#stratégies-de-sauvegarde)
    - [Module de Logging (DLL)](#module-de-logging-dll)
    - [Gestion de l'État en Temps Réel](#gestion-de-letat-en-temps-réel)
    - [Interface Utilisateur Bilingue](#interface-utilisateur-bilingue)
- [Tests et Gestion des Erreurs](#tests-et-gestion-des-erreurs)
- [Documentation pour le Support Technique](#documentation-pour-le-support-technique)
- [FAQ](#faq)
- [Contribuer](#contribuer)
- [Historique des Versions](#historique-des-versions)

---

## Introduction

**EasySave** est une application console de sauvegarde développée en C# avec .NET Core.  
Elle permet de gérer jusqu'à **5 backups** simultanément, avec deux stratégies de sauvegarde disponibles :

- **Sauvegarde Complète (Full)** : Copie tous les fichiers du répertoire source vers le répertoire cible.
- **Sauvegarde Différentielle (Diff)** : Copie uniquement les fichiers modifiés depuis la dernière sauvegarde complète.

L'application intègre également :

- **Un module de journalisation** qui enregistre toutes les actions réalisées pendant la sauvegarde dans un fichier log journalier au format JSON.
- **Un suivi de l'état en temps réel** de chaque backup via un fichier `state.json`.
- **Une interface console conviviale et bilingue** (Français / Anglais) avec vérification de chaque saisie utilisateur.
- **La possibilité de lancer l'application via la ligne de commande** en passant des arguments pour exécuter automatiquement certains backups.

---

## Fonctionnalités

- **Création de Backups :**
    - Chaque backup est défini par :
        - Un **nom**
        - Un **répertoire source** (contenant tous les fichiers et sous-dossiers)
        - Un **répertoire cible**
        - Un **type** de sauvegarde ("full" ou "diff")
- **Exécution de Backups :**
    - Possibilité d'exécuter un backup individuel ou tous les backups séquentiellement.
- **Journalisation en Temps Réel :**
    - Enregistrement de chaque opération (copie de fichier, création de dossier, etc.) dans un fichier log journalier nommé au format `yyyy-MM-dd-Log.json`.
    - Chaque entrée de log contient :
        - Le **nom** du backup
        - Le **chemin complet** du fichier source (format UNC recommandé)
        - Le **chemin complet** du fichier cible (format UNC recommandé)
        - La **taille** du fichier (avec unité `"octets"`)
        - Le **temps de transfert** en millisecondes (avec unité `"ms"`)
        - L’**horodatage** de l’opération (format `dd/MM/yyyy HH:mm:ss`)
- **Suivi de l'État en Temps Réel :**
    - Mise à jour d'un fichier `state.json` avec les informations suivantes pour chaque backup :
        - **Nom** du backup
        - **Chemins** source et cible en cours de traitement
        - **Status** (une énumération avec les valeurs `"Active"` et `"End"`, entre autres)
        - **Nombre total de fichiers** à copier
        - **Taille totale** de tous les fichiers à copier (avec unité `"octets"`)
        - **Nombre de fichiers restants** à copier
        - **Progression** en pourcentage (0 à 100)
        - **Dernier horodatage** d'action (`LastActionTimestamp`)
- **Interface Console Améliorée et Bilingue :**
    - Vérification et validation de chaque saisie (nom, chemins, type, indice)
    - Sélection de la langue au démarrage (Français ou Anglais)
- **Lancement en Ligne de Commande :**
    - Possibilité de lancer l’application avec des arguments pour exécuter directement certains backups.
    - Exemple d'argument : `"1-3"` pour exécuter les backups 1 à 3 ou `"1;3"` pour exécuter les backups 1 et 3 uniquement.
- **Module de Logging Isolé dans une DLL :**
    - La fonctionnalité de logging est encapsulée dans une DLL séparée (`EasySave.Logging`), facilitant sa maintenance et son évolution.

---

## Architecture

### Vue d'ensemble

Le projet EasySave est organisé en plusieurs modules afin de respecter la séparation des responsabilités :

- **Controllers :** Gère la logique métier et l’interaction entre la vue et le modèle (ex. `BackupController`).
- **Models :** Contient les classes principales (ex. `Backup`, `BackupManager`, `BackupState`, `IBackupStrategy`, `FullBackupStrategy`, `DifferentialBackupStrategy`).
- **Views :** Contient l’interface utilisateur console (`ConsoleView`).
- **Logging :** Module de journalisation (dans la DLL **EasySave.Logging**) avec l’interface `IBackupLogger` et son implémentation `JsonBackupLogger`.
- **Localization/Utils :** Fichiers de messages multilingues (ex. `Messages.cs`).

### Diagramme de Classes


### Diagramme de Séquence


---

## Installation et Configuration

### Prérequis

- **.NET 9.0 SDK** 
- IDE recommandé : [Rider](https://www.jetbrains.com/rider/) ou Visual Studio 2022+
- Git (pour la gestion de version)

### Installation

1. **Cloner le Dépôt :**

   ```bash
   git clone https://github.com/JulesBlanchard/EasySave.git
   cd EasySave
   
2. **Ouvrir la Solution :**

Dans Rider, ouvrez le fichier EasySave.sln. Cela charge tous les projets (y compris le projet de logging EasySave.Logging).

3. **Compiler la Solution :**

Dans Rider, sélectionnez Build > Rebuild Solution pour compiler tous les projets.

# Utilisation de l'Application

EasySave est une application de sauvegarde en mode console qui peut être utilisée en mode interactif ou lancée directement via la ligne de commande avec des arguments. Cette section décrit en détail les deux modes de fonctionnement ainsi que quelques exemples d'utilisation.

---

## Mode Interactif

En l'absence d'arguments en ligne de commande, EasySave démarre en mode interactif. Dans ce mode, l'application vous guide à travers un menu et vous demande de saisir les informations nécessaires pour créer et exécuter des backups.

### Fonctionnement

1. **Démarrage de l'application**
    - Lancez EasySave (par exemple, via Rider ou en exécutant la commande `dotnet EasySave.dll` sans arguments).
    - L'application affiche une invitation à sélectionner la langue (Français ou English).

2. **Affichage du Menu**  
   Une fois la langue choisie, le menu principal s'affiche, par exemple :
    - **1. Créer un backup**
    - **2. Lister les backups**
    - **3. Exécuter un backup**
    - **4. Quitter**

3. **Création d'un Backup**
    - Si vous choisissez l'option 1, l'application vous demande :
        - Le **nom** du backup.
        - Le **chemin source** (le répertoire source doit exister).
        - Le **chemin cible** (si le répertoire n'existe pas, vous serez invité à le créer).
        - Le **type de sauvegarde** (entrez "full" pour une sauvegarde complète ou "diff" pour une sauvegarde différentielle).
    - Une fois ces informations validées, le backup est créé et enregistré dans le fichier `backups.json`.

4. **Exécution d'un Backup**
    - L'option 3 vous permet d'exécuter un backup.
    - Après avoir listé les backups existants, l'application vous demande de saisir l'indice du backup à exécuter (les indices affichés commencent à 1).
    - L'application convertit cet indice en 0-indexé et exécute le backup correspondant, en effectuant la copie des fichiers selon la stratégie sélectionnée et en mettant à jour l'état en temps réel via `state.json`.

5. **Liste des Backups**
    - L'option 2 affiche la liste des backups existants, avec le nom, la stratégie, le chemin source et le chemin cible.

---

## Lancement en Ligne de Commande

EasySave prend également en charge le lancement direct depuis la ligne de commande. Si vous fournissez des arguments lors du démarrage, l'application ne démarre pas le mode interactif, mais exécute directement les backups correspondants.

### Comment cela fonctionne

- **Argument de lancement :**  
  Lorsque vous lancez l'application avec un argument, le programme interprète cet argument pour déterminer quels backups exécuter.  
  L'argument peut être dans l'un des formats suivants :
    - **Plage :** `"1-3"` pour exécuter les backups 1, 2 et 3.
    - **Liste :** `"1;3"` pour exécuter uniquement les backups 1 et 3.
    - **Indice unique :** `"2"` pour exécuter uniquement le backup 2.

- **Conversion des indices :**  
  Les indices fournis par l'utilisateur sont supposés être 1-indexés. Le programme les convertit en indices 0-indexés pour accéder aux backups dans la liste.

### Commande de lancement

Pour lancer EasySave depuis la ligne de commande, ouvrez un terminal dans le dossier contenant le fichier `EasySave.dll` (ou l'exécutable) et utilisez la commande suivante :

```bash
dotnet EasySave.dll "argument"
```
où "argument" est par exemple "1-3", "1;3" ou "2".

#### Où se placer ? 

- Pour exécuter la commande il faut se placer au niveau de répertoire : 
```bash
EasySave\EasySave\bin\Debug\net9.0
```
Car c'est dans ce dernier que se trouve EasySave.dll. Par la suite on pensera à publier l'application afin de pouvoir lancer cette ligne de commande depuis partout


