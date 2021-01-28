$client = new-object System.Net.WebClient
$client.DownloadFile("https://ci.appveyor.com/api/projects/Thalitech/Nami/artifacts/Nami.zip","Nami.zip")
$client.DownloadFile("https://ci.appveyor.com/api/projects/Thalitech/Nami/artifacts/NamiResources.zip","NamiResources.zip")