namespace Cegeka.Updater.Service
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.CegekaUpdateServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.CegekaUpdateServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // CegekaUpdateServiceProcessInstaller
            // 
            this.CegekaUpdateServiceProcessInstaller.Password = null;
            this.CegekaUpdateServiceProcessInstaller.Username = null;
            // 
            // CegekaUpdateServiceInstaller
            // 
            this.CegekaUpdateServiceInstaller.Description = "Provides centralized management of windows updates.";
            this.CegekaUpdateServiceInstaller.DisplayName = "Cegeka Update Service";
            this.CegekaUpdateServiceInstaller.ServiceName = "CGKUpdateService";
            this.CegekaUpdateServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.CegekaUpdateServiceProcessInstaller,
            this.CegekaUpdateServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller CegekaUpdateServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller CegekaUpdateServiceInstaller;
    }
}