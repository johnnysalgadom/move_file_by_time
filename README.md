# move_file_by_time

Objetivo:
Servicio de Windows que mueve archivos en base a su permanencia en una carpeta inicial.
Se mueve a una segunda carpeta en base a un tiempo t1 determinado.
Luego que pasa un tiempo t2 se retorna a la carpeta inicial, finalmente si pasa un tiempo t3 se mueve a una tercera carpeta.

Nombre del Servicio:
El nombre que se mostrará en el Administrador de Servicio de Windows es "Mover archivos a carpeta según el tiempo transcurrido"

Configuración:
En el archivo App.config se deben establecer los valores para las variables de tiempo y rutas de las carpetas
  <appSettings>
    <!-- Intervalo del servicio-->
    <add key="TimerInterval" value="1" />
    <!-- carpeta inicial -->
    <add key="FolderA" value="D:\\backtech\windows_service\\folder_a" />
    <!-- segunda carpeta -->
    <add key="FolderB" value="D:\\backtech\windows_service\\folder_b" />
    <!-- carpeta final -->
    <add key="FolderC" value="D:\\backtech\windows_service\\folder_c" />
    <!--ControlTime1 expresado en minutos. Por ejemplo 1 dia es 1440 minutos, colocar 1440-->
    <add key="ControlTime1" value="2" />
    <!--ControlTime1 expresado en minutos. Por ejemplo 2 dias es 2880 minutos, colocar 2880-->
    <add key="ControlTime2" value="2" />
    <!--ControlTime3 Expresado en minutos. -->
    <add key="ControlTime3" value="2" />
  </appSettings>

  Ejecución:
  - Abrir una ventana CMD
  - Ir a C:\Windows\Microsoft.NET\Framework\v4.0.30319\
  - Ejecutar InstallUtil.exe <RutaDelObjeto>\MoveFileByTime.exe
  - Ir al administrador de servicios de Windows
  - Buscar el servicio "Mover archivos a carpeta según el tiempo transcurrido"
  - Iniciar servicio
  - Si desea desinstalar, debe detener el servicio y ejcutar InstallUtil.exe -D <RutaDelObjeto>\MoveFileByTime.exe