FROM golang:1.24 as builder

WORKDIR /app

ENV GOPATH=/go
ENV PATH=$GOPATH/bin:$PATH

# Install xk6 and build k6 with Kafka extension
RUN go install go.k6.io/xk6/cmd/xk6@latest
RUN xk6 build --with github.com/mostafa/xk6-kafka@latest

FROM alpine:3.18
COPY --from=builder /app/k6 /usr/bin/k6

ENTRYPOINT ["k6"]