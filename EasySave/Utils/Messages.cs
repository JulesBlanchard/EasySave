using System.Collections.Generic;

namespace EasySave.Utils
{
    /// <summary>
    /// Provides bilingual messages for the user interface and other outputs.
    /// </summary>
    public static class Messages
    {
        // French messages.
        public static Dictionary<string, string> French = new Dictionary<string, string>
        {
            // Menu
            { "MenuTitle", "=== EasySave ===" },
            { "MenuOption1", "1. Créer une sauvegarde" },
            { "MenuOption2", "2. Lister les sauvegardes" },
            { "MenuOption3", "3. Exécuter une sauvegarde" },
            { "MenuOption4", "4. Supprimer une sauvegarde" },
            { "MenuOption5", "5. Exécuter toutes les sauvegardes" },
            { "MenuOption6", "6. Quitter" },
            { "PromptChoice", "Choix : " },
            { "InvalidOption", "Option inconnue. Veuillez réessayer." },

            // Language selection
            { "LanguagePrompt", "Choisissez la langue / Select language :" },
            { "LanguageOption1", "1. Français" },
            { "LanguageOption2", "2. English" },
            { "LanguageChoice", "Votre choix / Your choice : " },

            // Backup creation
            { "BackupCreationTitle", "=== Création d'un Backup ===" },
            { "EnterBackupName", "Nom de la sauvegarde : " },
            { "EnterSourcePath", "Chemin source : " },
            { "EnterTargetPath", "Chemin cible : " },
            { "EnterBackupType", "Type de sauvegarde (full/diff) : " },
            { "BackupCreated", "sauvegarde créé avec succès !" },
            { "InvalidInput", "L'entrée ne peut pas être vide. Veuillez réessayer." },
            { "SourceNotExist", "Le répertoire source n'existe pas. Veuillez vérifier le chemin." },
            { "TargetNotExist", "Le répertoire cible n'existe pas." },
            { "CreateTargetPrompt", "Voulez-vous le créer ? (O/N) : " },
            { "TargetCreated", "Répertoire créé." },
            { "InvalidBackupType", "Type invalide : veuillez choisir (full/diff) : " },
            { "DifferentTargetPath", "Veuillez entrer un chemin cible différent" },



            // Backup execution
            { "BackupExecutionTitle", "=== Exécution d'une sauvegarde ===" },
            { "BackupDeletionTitle", "=== Suppression d'une sauvegarde ===" },
            { "EnterBackupIndex", "Indice de la sauvegarde : " }, //exec et delete
            { "EnterValidNumber", "Veuillez entrer un nombre valide." },
            { "NoBackupsToExecute", "Pas de sauvegarde à exécuter" },


            // Controller messages
            { "ControllerBackupCreated", "Sauvegarde '{name}' créé avec stratégie={strategy}." },
            { "ControllerInvalidIndex", "Index invalide." },
            { "ControllerExecutingBackup", "Exécution du backup '{name}' (index={index})." },
            { "ControllerDeletingBackup", "La sauvegarde {index} a été supprimée." },
            { "ControllerExecutingAll", "Exécution de {count} sauvegarde(s)..." },
            { "ControllerBackupAdded", "Sauvegarde '{name}' ajouté. (count={count})" },
            {"NoBackupsToDelete", "Pas de sauvegarde à supprimer." },


            // DiffBackup strategy messages
            { "DiffBackup_Executing", "Exécution d'une sauvegarde DIFFÉRENTIELLE pour '{name}'..." },
            { "DiffBackup_Copied", "Copié : {name}" },
            { "DiffBackup_ErrorCopy", "Erreur lors de la copie de {name} : {error}" },
            { "DiffBackup_Skipped", "Ignoré (à jour) : {name}" },
            { "DiffBackup_Progress", "Progression : {progress}% - Temps restant estimé : {time}" },
            { "DiffBackup_Finished", "Terminé. {count} fichier(s) copié(s)." },

            // FullBackup strategy messages
            { "FullBackup_Executing", "Exécution d'une sauvegarde FULL pour '{name}'..." },
            { "FullBackup_Copied", "Copié : {name}" },
            { "FullBackup_ErrorCopy", "Erreur lors de la copie de {name} : {error}" },
            { "FullBackup_Progress", "Progression : {progress}% - Temps restant estimé : {time}" },
            { "FullBackup_Finished", "Sauvegarde terminée pour '{name}'." },

            // Listing backups
            { "ListBackupsTitle", "Sauvegardes courantes:" },
            
            { "BackupManager_LoadError", "Erreur lors du chargement des sauvegardes : {error}" }

        };

        // English messages.
        public static Dictionary<string, string> English = new Dictionary<string, string>
        {
            // Menu
            { "MenuTitle", "=== EasySave ===" },
            { "MenuOption1", "1. Create a backup" },
            { "MenuOption2", "2. List backups" },
            { "MenuOption3", "3. Execute a backup" },
            { "MenuOption4", "4. Delete a backup" },
            { "MenuOption5", "5. Execute all backups" },
            { "MenuOption6", "6. Quit" },
            { "PromptChoice", "Choice: " },
            { "InvalidOption", "Unknown option. Please try again." },

            // Language selection
            { "LanguagePrompt", "Select language / Choisissez la langue:" },
            { "LanguageOption1", "1. Français" },
            { "LanguageOption2", "2. English" },
            { "LanguageChoice", "Your choice / Votre choix: " },

            // Backup creation
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
            { "InvalidBackupType", "Invalid type. Please enter 'full' or 'diff'." },
            { "DifferentTargetPath", "Please enter a different target path." },



            // Backup execution
            { "BackupExecutionTitle", "=== Execute a Backup ===" },
            { "BackupDeletionTitle", "=== Delete a Backup ===" },
            { "EnterBackupIndex", "Enter backup index : " },
            { "EnterValidNumber", "Please enter a valid number." },
            { "NoBackupsToExecute", "No backups to execute." },


            // Controller messages
            { "ControllerBackupCreated", "Backup '{name}' created with strategy={strategy}." },
            { "ControllerInvalidIndex", "Invalid index." },
            { "ControllerExecutingBackup", "Executing backup '{name}' (index={index})." },
            { "ControllerDeletingBackup", "Backup {index} deleted." },
            { "ControllerExecutingAll", "Executing {count} backup(s)..." },
            { "ControllerBackupAdded", "Backup '{name}' added. (count={count})" },
            {"NoBackupsToDelete", "No backups to delete." },

            // DiffBackup strategy messages
            { "DiffBackup_Executing", "Executing differential backup for '{name}'..." },
            { "DiffBackup_Copied", "Copied: {name}" },
            { "DiffBackup_ErrorCopy", "Error copying {name}: {error}" },
            { "DiffBackup_Skipped", "Skipped (up-to-date): {name}" },
            { "DiffBackup_Progress", "Progress: {progress}% - Estimated time remaining: {time}" },
            { "DiffBackup_Finished", "Finished. {count} file(s) copied." },

            // FullBackup strategy messages
            { "FullBackup_Executing", "Executing full backup for '{name}'..." },
            { "FullBackup_Copied", "Copied: {name}" },
            { "FullBackup_ErrorCopy", "Error copying {name}: {error}" },
            { "FullBackup_Progress", "Progress: {progress}% - Estimated time remaining: {time}" },
            { "FullBackup_Finished", "Backup completed for '{name}'." },

            // Listing backups
            { "ListBackupsTitle", "Current Backups:" },
            
            { "BackupManager_LoadError", "Error loading backups: {error}" }
        };
    }
}
