# docker build

C:\_code\LogStreamer
docker build -f .\LogProducer\Dockerfile -t davevans/logproducer:1.0.0 .
docker push davevans/logproducer:1.0.0

## K8s
kubectl run logproducer --image=davevans/logproducer:1.0.0
