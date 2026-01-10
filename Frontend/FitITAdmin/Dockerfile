FROM node:18-alpine AS build

# Build SharedUI
WORKDIR /SharedUI
COPY Frontend/SharedUI/package*.json ./
RUN npm install
COPY Frontend/SharedUI/ .
RUN npm run build

# Build App
WORKDIR /app
COPY Frontend/FitITAdmin/package*.json ./
RUN npm install
COPY Frontend/FitITAdmin/ .
RUN npm run build

FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY Frontend/FitITAdmin/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
