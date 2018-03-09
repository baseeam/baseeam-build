$cnfFile = "D:\OPENIT\baseeam-build\AutomationScripts\my.cnf" #Config file
$backupDir = "D:\OPENIT\baseeam-build\Database" #Backup Directory
$mysqldump = "C:\Program Files\MySQL\MySQL Server 5.7\bin\mysqldump.exe" #Patch to mysqldump.exe
$mysqlDataDir = "C:\Documents and Settings\All Users\MySQL\MySQL Server 5.7\data" #Patch to datatbases files directory

#Get only names of the databases folders
$sqlDbDirList = ls -path $mysqlDataDir | ?{ $_.PSIsContainer } | Select-Object Name
foreach($dbDir in $sqlDbDirList) {
	if(!($dbDir.Name -Match "EAM")) {
		break
	}
    $dbBackupDir = $backupDir + "\" + $dbDir.Name
    #If folder not exist, create it
    if (!(Test-Path -path $dbBackupDir -PathType Container)) {
        New-Item -Path $dbBackupDir -ItemType Directory
    }
    
    $dbBackupFile = $dbBackupDir + "\" + $dbDir.Name + "_" + (Get-Date -format "yyyyMMdd_HHmmss")
    #Dump to sql file and archive it
    $sqlFile = $dbBackupFile + ".sql"
    & $mysqldump --defaults-extra-file=$cnfFile -B $dbDir.Name -r $sqlFile
}
