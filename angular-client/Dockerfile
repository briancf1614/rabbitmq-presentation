# ETAPA 1: Construcción (Build)
FROM node:22-alpine as build

WORKDIR /app

# Copiamos los archivos de dependencias
COPY package*.json ./

# Instalamos dependencias
RUN npm install

# Copiamos el resto del código
COPY . .

# Construimos la app para producción
RUN npm run build -- --configuration production

# ETAPA 2: Servidor Web (Nginx)
FROM nginx:alpine

# Copiamos el resultado de la build a la carpeta de Nginx
# IMPORTANTE: Asegúrate de que 'rabbitmq-presentation' coincide con el "outputPath" en tu angular.json
# Si no estás seguro, usa un 'ls' en la etapa anterior o revisa tu angular.json
COPY --from=build /app/dist/*/browser /usr/share/nginx/html
# Si tu angular es versión vieja (antes de v17), la ruta es /app/dist/NOMBRE_PROYECTO

# Exponemos el puerto 80
EXPOSE 80

# Arrancamos Nginx
CMD ["nginx", "-g", "daemon off;"]
