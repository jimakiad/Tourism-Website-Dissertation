import React, { useState, useCallback, useEffect, useRef } from "react";
import { RMap, ROSM, RLayerVector, RFeature, RPopup } from "rlayers";
import { Point } from "ol/geom";
import { fromLonLat, toLonLat } from "ol/proj";
import "ol/ol.css";

const MapInput = ({ onPositionChange, initialPosition = null }) => {
	const [coords, setCoords] = useState(() =>
		initialPosition
			? fromLonLat([initialPosition.longitude, initialPosition.latitude])
			: null,
	);
	const mapRef = useRef(null);

	const defaultCenterLonLat = [10, 50];
	const defaultZoom = 4;
	const selectedZoom = 13;

	const initialViewState = {
		center: coords || fromLonLat(defaultCenterLonLat),
		zoom: coords ? selectedZoom : defaultZoom,
	};

	const handleMapClick = useCallback((e) => {
		const clickedCoords = e.coordinate;
		console.log("OL Map Clicked - Map Coords:", clickedCoords);
		setCoords(clickedCoords);

		const map = e.map;
		if (map) {
			map
				.getView()
				.animate({ center: clickedCoords, zoom: selectedZoom, duration: 300 });
		}
	}, []);

	useEffect(() => {
		console.log("MapInput useEffect [coords]: State changed to", coords);
		if (onPositionChange) {
			let positionForParent = null;
			if (coords) {
				const lonLat = toLonLat(coords);
				positionForParent = { latitude: lonLat[1], longitude: lonLat[0] };
			}
			console.log(
				"MapInput: Calling onPositionChange with Lat/Lon:",
				positionForParent,
			);
			onPositionChange(positionForParent);
		}
	}, [coords, onPositionChange]);

	const displayCoords = coords ? toLonLat(coords) : null;

	return (
		<div
			className="border rounded mb-4 overflow-hidden"
			style={{ height: "300px", width: "100%" }}
		>
			<RMap
				ref={mapRef}
				className="h-full w-full"
				initial={initialViewState}
				onClick={handleMapClick}
			>
				<ROSM />
				<RLayerVector>
					{coords && (
						<RFeature
							geometry={new Point(coords)}
						>
							<RPopup trigger={"click"} className="p-2">
								<span className="text-sm">Location Selected</span>
							</RPopup>
						</RFeature>
					)}
				</RLayerVector>
			</RMap>
			{displayCoords && (
				<p className="text-xs text-center text-gray-600 p-1 bg-gray-100">
					Selected: Lat: {displayCoords[1].toFixed(5)}, Lng:{" "}
					{displayCoords[0].toFixed(5)}
				</p>
			)}
			{!displayCoords && (
				<p className="text-xs text-center text-gray-500 p-1 bg-gray-100">
					Click on the map to select a location (Optional)
				</p>
			)}
		</div>
	);
};

export default MapInput;
