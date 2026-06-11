# ---- Build ----
FROM node:22-alpine AS build
WORKDIR /app
# Garante a instalação das devDependencies (onde está o @angular/cli),
# mesmo que NODE_ENV venha como production no ambiente.
ENV NODE_ENV=development
COPY package*.json ./
RUN npm ci --no-audit --no-fund
COPY . .
RUN npm run build

# ---- Runtime (nginx) ----
FROM nginx:alpine AS final
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/sistemaan/browser /usr/share/nginx/html
EXPOSE 80
