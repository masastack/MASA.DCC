param($t, $u, $p)

#docker build 
#Write-Host "Hello.$args"
Write-Host "Hello $t"

docker login --username=$u registry.cn-hangzhou.aliyuncs.com --password=$p

$ServiceDockerfilePath = "./src/Services/Masa.Dcc.Service/Dockerfile"
$ServiceServerName = "masa-dcc-service-admin"
$WebDockerfilePath = "./src/Web/Masa.Dcc.Web.Admin/Masa.Dcc.Web.Admin.Server/Dockerfile"
$WebServerName = "masa-dcc-web-admin"

docker build -t registry.cn-hangzhou.aliyuncs.com/masastack/${WebServerName}:$t  -f $WebDockerfilePath .
docker push registry.cn-hangzhou.aliyuncs.com/masastack/${WebServerName}:$t 

docker build -t registry.cn-hangzhou.aliyuncs.com/masastack/${ServiceServerName}:$t  -f $ServiceDockerfilePath .
docker push registry.cn-hangzhou.aliyuncs.com/masastack/${ServiceServerName}:$t 