# ---- Build ----
# Debian (glibc) é mais compatível com os binários nativos do build do Angular
# do que o alpine (musl), evitando falhas no 'ng build'.
FROM node:22-slim AS build
WORKDIR /app
# Garante a instalação das devDependencies (onde está o @angular/cli/ng).
ENV NODE_ENV=development
COPY package*.json ./
RUN npm ci --include=dev --no-audit --no-fund
COPY . .
RUN npm run build

# ---- Runtime (nginx) ----
FROM nginx:alpine AS final
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/sistemaan/browser /usr/share/nginx/html
EXPOSE 80
