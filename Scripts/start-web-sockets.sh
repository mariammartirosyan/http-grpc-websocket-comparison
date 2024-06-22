apply_kubectl() {
    file=$1
    kubectl apply -f "$file"
    if [ $? -ne 0 ]; then
        echo "Error applying $file."
    fi
}
echo "Deploying the databases"

echo "Deploying the account database"
apply_kubectl "../WebSockets/Manifest Files/account-db.yaml"

echo "Deploying the movie database"
apply_kubectl "../WebSockets/Manifest Files/movie-db.yaml"

echo "Deploying the statistics database"
apply_kubectl "../WebSockets/Manifest Files/statistics-db.yaml"

echo "Installing NGINX Ingress"
helm repo add ingress-nginx https://kubernetes.github.io/ingress-nginx 
helm repo update
helm install my-release ingress-nginx/ingress-nginx --namespace default

echo "Deploying the microservices"
kubectl wait --for='jsonpath={.status.conditions[?(@.type=="Ready")].status}=True' deployment.apps/account-service-web-socket-deployment --timeout=100s

apply_kubectl "../WebSockets/Manifest Files/account.yaml" 
apply_kubectl "../WebSockets/Manifest Files/movie.yaml" 
apply_kubectl "../WebSockets/Manifest Files/statistics.yaml" 
apply_kubectl "../WebSockets/Manifest Files/trailer-streaming.yaml" 

echo "Deploying the client app"
apply_kubectl "../WebSockets/Manifest Files/client.yaml" 

apply_kubectl "../WebSockets/Manifest Files/ingress.yaml" 