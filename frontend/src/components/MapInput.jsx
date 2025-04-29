// src/components/MapInput.jsx
import React, { useState, useCallback, useEffect, useRef } from "react";
import { RMap, ROSM, RLayerVector, RFeature, RPopup } from "rlayers";
import { Point } from "ol/geom";
import { fromLonLat, toLonLat } from "ol/proj"; // Import projection functions
import "ol/ol.css"; // Import OpenLayers CSS

const MapInput = ({ onPositionChange, initialPosition = null }) => {
	// State stores coordinates in the map's projection (usually EPSG:3857)
	// Convert initial position (Lat/Lon) to map projection on init
	const [coords, setCoords] = useState(() =>
		initialPosition
			? fromLonLat([initialPosition.longitude, initialPosition.latitude])
			: null,
	);
	// Ref to the RMap component instance (optional, useful for map methods)
	const mapRef = useRef(null);

	// Default center (Lon/Lat) and zoom
	const defaultCenterLonLat = [10, 50];
	const defaultZoom = 4;
	const selectedZoom = 13;

	// Calculate initial map view state
	const initialViewState = {
		center: coords || fromLonLat(defaultCenterLonLat),
		zoom: coords ? selectedZoom : defaultZoom,
	};

	// Handler for map clicks provided by RMap's onClick prop
	// The event 'e' contains 'coordinate' in the map's projection
	const handleMapClick = useCallback((e) => {
		const clickedCoords = e.coordinate;
		console.log("OL Map Clicked - Map Coords:", clickedCoords);
		setCoords(clickedCoords); // Update state with map coordinates

		// Optional: Fly map to the clicked location
		const map = e.map; // Get map instance from event
		if (map) {
			map
				.getView()
				.animate({ center: clickedCoords, zoom: selectedZoom, duration: 300 });
		}
	}, []); // Empty dependency array - function is stable

	// Effect to notify parent component when 'coords' state changes
	useEffect(() => {
		console.log("MapInput useEffect [coords]: State changed to", coords);
		if (onPositionChange) {
			let positionForParent = null;
			if (coords) {
				// Convert map coordinates (EPSG:3857) back to Lat/Lon (EPSG:4326)
				const lonLat = toLonLat(coords);
				positionForParent = { latitude: lonLat[1], longitude: lonLat[0] };
			}
			console.log(
				"MapInput: Calling onPositionChange with Lat/Lon:",
				positionForParent,
			);
			onPositionChange(positionForParent);
		}
	}, [coords, onPositionChange]); // Run when coords or the callback changes

	// Convert current coords back to display Lat/Lon below map
	const displayCoords = coords ? toLonLat(coords) : null;

	return (
		<div
			className="border rounded mb-4 overflow-hidden"
			style={{ height: "300px", width: "100%" }}
		>
			<RMap
				ref={mapRef} // Assign ref
				className="h-full w-full"
				initial={initialViewState} // Set initial center and zoom
				onClick={handleMapClick} // Use the built-in onClick handler
			>
				<ROSM /> {/* Standard OpenStreetMap Tiles */}
				{/* Layer to hold the vector features (our marker) */}
				<RLayerVector>
					{/* Render the feature (marker) only if coords exist */}
					{coords && (
						<RFeature
							geometry={new Point(coords)} // Create an OpenLayers Point geometry
							// onClick={ (e) => e.map.getView().fit(e.target.getGeometry().getExtent(), { duration: 250, maxZoom: 15 }) } // Optional: Zoom on marker click
						>
							{/* Optional: Popup displayed when marker is clicked */}
							<RPopup trigger={"click"} className="p-2">
								<span className="text-sm">Location Selected</span>
							</RPopup>
						</RFeature>
					)}
				</RLayerVector>
			</RMap>
			{/* Display selected coordinates */}
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
