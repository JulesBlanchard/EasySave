using System.Collections.Generic;

namespace EasySave.Utils
{
    public static class Messages
    {
        // Messages en français
        public static Dictionary<string, string> French = new Dictionary<string, string>
        {
            { "MenuTitle", "=== EasySave ===" },
            { "MenuOption1", "1. Créer un backup" },
            { "MenuOption2", "2. Lister les backups" },
            { "MenuOption3", "3. Exécuter un backup" },
            { "MenuOption4", "4. Quitter" },
            { "PromptChoice", "Choix : " },
            { "InvalidOption", "Option inconnue. Veuillez réessayer." },
            { "LanguagePrompt", "Choisissez la langue / Select language :" },
            { "LanguageOption1", "1. Français" },
            { "LanguageOption2", "2. English" },
            { "LanguageChoice", "Votre choix / Your choice : " },
            { "BackupCreationTitle", "=== Création d'un Backup ===" },
            { "EnterBackupName", "Nom du backup : " },
            { "EnterSourcePath", "Chemin source : " },
            { "EnterTargetPath", "Chemin cible : " },
            { "EnterBackupType", "Type de sauvegarde (full/diff) : " },
            { "BackupCreated", "Backup créé avec succès !" },
            { "InvalidInput", "L'entrée ne peut pas être vide. Veuillez réessayer." },
            { "SourceNotExist", "Le répertoire source n'existe pas. Veuillez vérifier le chemin." },
            { "TargetNotExist", "Le répertoire cible n'existe pas." },
            { "CreateTargetPrompt", "Voulez-vous le créer ? (O/N) : " },
            { "TargetCreated", "Répertoire créé." },
            { "BackupExecutionTitle", "=== Exécution d'un Backup ===" },
            { "EnterBackupIndex", "Indice du backup à exécuter : " },
            { "EnterValidNumber", "Veuillez entrer un nombre valide." },
            { "CannotCreateMoreBackups", "Impossible de créer plus de 5 backups." }
        };

        // Messages en anglais
        public static Dictionary<string, string> English = new Dictionary<string, string>
        {
            { "MenuTitle", "=== EasySave ===" },
            { "MenuOption1", "1. Create a backup" },
            { "MenuOption2", "2. List backups" },
            { "MenuOption3", "3. Execute a backup" },
            { "MenuOption4", "4. Quit" },
            { "PromptChoice", "Choice: " },
            { "InvalidOption", "Unknown option. Please try again." },
            { "LanguagePrompt", "Select language / Choisissez la langue:" },
            { "LanguageOption1", "1. Français" },
            { "LanguageOption2", "2. English" },
            { "LanguageChoice", "Your choice / Votre choix: " },
            { "BackupCreationTitle", "=== Create a Backup ===" },
            { "EnterBackupName", "Backup Name: " },
            { "EnterSourcePath", "Source Path: " },
            { "EnterTargetPath", "Target Path: " },
            { "EnterBackupType", "Backup Type (full/diff): " },
            { "BackupCreated", "Backup created successfully!" },
            { "InvalidInput", "Input cannot be empty. Please try again." },
            { "SourceNotExist", "Source directory does not exist. Please check the path." },
            { "TargetNotExist", "Target directory does not exist." },
            { "CreateTargetPrompt", "Do you want to create it? (Y/N): " },
            { "TargetCreated", "Directory created." },
            { "BackupExecutionTitle", "=== Execute a Backup ===" },
            { "EnterBackupIndex", "Enter backup index to execute: " },
            { "EnterValidNumber", "Please enter a valid number." },
            { "CannotCreateMoreBackups", "Cannot create more than 5 backups." }
        };
    }
}
