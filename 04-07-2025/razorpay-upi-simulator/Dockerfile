# Stage 1: Build Angular app
FROM node:alpine AS build-stage

WORKDIR /app

COPY package.json package-lock.json ./
RUN npm install

COPY . .
RUN npm run build -- --configuration=production


FROM nginx:alpine

# Remove default NGINX website
RUN rm -rf /usr/share/nginx/html/*


COPY --from=build-stage /app/dist/razorpay-upi-simulator/browser /usr/share/nginx/html

RUN chmod -R 755 /usr/share/nginx/html

COPY ./nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
