import Lottie from "lottie-react";
import loadingAnimation from "./assets/Loading.json";

export default function LoadingSpinner({ size = 180 }) {
  return (
    <div className="d-flex align-items-center justify-content-center py-5">
      <div style={{ width: size, height: size }}>
        <Lottie animationData={loadingAnimation} loop={true} />
      </div>
    </div>
  );
}
